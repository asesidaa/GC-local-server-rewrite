# Native Mediator Migration Design

## Context

The solution currently depends on MediatR 14.1.0 for in-process request dispatch. The migration will replace it with the MIT-licensed, source-generated [martinothamar/Mediator](https://github.com/martinothamar/Mediator), pinned to the latest stable 3.0.2 packages. This is an atomic migration: no MediatR compatibility package, namespace alias, adapter, shim, or dual-registration period will remain.

The current surface comprises 73 wrapped request files, three direct request types, 77 controller send calls, and two cross-cutting pipeline concerns. The solution does not use mediator notifications or streaming requests.

## Goals

- Remove every MediatR package reference and source symbol.
- Model existing commands and queries with Mediator's native `ICommand`/`IQuery` contracts.
- Use generated registration and dispatch without reflection-based compatibility APIs.
- Preserve all HTTP routes, protocol payloads, business behavior, and persistence semantics.
- Retain request logging and unhandled-exception logging in the native pipeline.
- Add focused dispatch coverage and verify build, test, and publish output.

## Non-Goals

- Changing controller routes, DTOs, XML serialization, database schemas, or game behavior.
- Introducing notifications, streaming, Native AOT publishing, or mediator telemetry.
- Refactoring unrelated application features or existing service-location patterns in controllers.

## Dependencies and Source Generation

`Application/Application.csproj` will replace `MediatR` with:

- `Mediator.Abstractions` 3.0.2 for messages, handlers, sender, and pipeline types.
- `Mediator.SourceGenerator` 3.0.2 as a private analyzer asset.

The generator belongs in `Application` because that project owns all messages, handlers, pipeline behaviors, and `AddApplication()`. This follows the library's clean-architecture sample and produces one mediator implementation for the deployed application.

`AddApplication()` will call generated `AddMediator()` with the Application assembly explicitly selected, `ServiceLifetime.Scoped`, and an ordered pipeline behavior list. Scoped lifetime is required because most handlers consume scoped EF Core contexts through `ICardDependencyAggregate`; the library's singleton default must not be used.

## Native Contract Model

The existing `IRequestWrapper<T>` and `IRequestHandlerWrapper<TRequest, TResponse>` will be replaced by four project-level abstractions:

- `IQueryWrapper<T>` extends `IQuery<ServiceResult<T>>`.
- `IQueryHandlerWrapper<TQuery, T>` extends `IQueryHandler<TQuery, ServiceResult<T>>`.
- `ICommandWrapper<T>` extends `ICommand<ServiceResult<T>>`.
- `ICommandHandlerWrapper<TCommand, T>` extends `ICommandHandler<TCommand, ServiceResult<T>>`.

These wrappers encode the project's `ServiceResult<T>` response policy; they do not emulate MediatR. Files and records ending in `Query` will use query contracts, while those ending in `Command` will use command contracts. The three direct contracts will migrate similarly: `GetDataQuery` and `PlayCountQuery` become native queries, and `CertifyCommand` becomes a native command.

The current shared handler base will become a non-generic dependency/helper base plus native `QueryHandlerBase<TQuery, T>` and `CommandHandlerBase<TCommand, T>` subclasses. This keeps EF/configuration helpers in one place while allowing generated handler discovery through the correct native interfaces.

## Handler and Pipeline Semantics

All public handler boundaries will return `ValueTask<T>`, as required by Mediator. Asynchronous EF-backed handlers will use `async ValueTask<T>`; synchronous handlers will return `ValueTask.FromResult`. Internal EF helper methods may remain `Task<T>` because they are not mediator contracts.

`LoggingBehaviour<TMessage, TResponse>` will become a native `MessagePreProcessor<TMessage, TResponse>`. `UnhandledExceptionBehaviour<TMessage, TResponse>` will implement native `IPipelineBehavior<TMessage, TResponse>`, invoke `next(message, cancellationToken)`, log, and rethrow. Both behaviors will be listed in generated configuration so ordering and registration are explicit.

`MainServer.Controllers.BaseController` will resolve `Mediator.ISender`. Existing controller calls continue to await `Send`; no HTTP or result conversion code changes.

## Documentation

Repository guidance will be updated from "MediatR" to "Mediator" and describe native command/query handlers and source-generated registration. Changes to the already-modified `CLAUDE.md` will be limited to mediator-specific lines so unrelated local edits remain intact.

## Testing and Verification

The existing xUnit project will gain dispatch tests that build the real Application service collection, create a scope, resolve `Mediator.ISender`, and send:

- `ValidateAdminLoginQuery` to prove native query registration and response handling.
- `CertifyCommand` to prove native command registration and response handling.

Tests will also confirm the two native pipeline behaviors are registered for a representative message. Verification will include:

1. `dotnet test GC-local-server-rewrite.sln -c Release`.
2. `dotnet build GC-local-server-rewrite.sln -c Release`.
3. A fresh Release `dotnet publish` of `MainServer` to a temporary directory.
4. A source scan showing no `MediatR` namespace or compatibility code.
5. A NuGet dependency listing showing no direct or transitive MediatR package.

## Risks and Controls

- **Scoped dependencies captured by singleton handlers:** explicitly configure scoped lifetime.
- **Incomplete generator discovery:** select the Application assembly and treat all generator diagnostics as build failures requiring correction.
- **Incorrect `ValueTask` conversion:** keep each result single-consumption and preserve `Task` only behind internal helper boundaries.
- **Pipeline behavior loss:** register both behaviors in generator options and cover registration/dispatch in tests.
- **Working-tree overlap:** patch only migration-owned lines and never stage unrelated changes.

## Success Criteria

The migration is complete when all command/query dispatch uses martinothamar/Mediator 3.0.2 directly, MediatR is absent from source and resolved dependencies, existing and new tests pass, and a fresh MainServer publish succeeds without compatibility artifacts.
