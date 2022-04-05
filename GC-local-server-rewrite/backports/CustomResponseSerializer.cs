﻿using EmbedIO;

namespace GCLocalServerRewrite.backports;

/// <summary>
///     Provides custom response serializer callbacks.
/// </summary>
public static class CustomResponseSerializer
{
    private static readonly ResponseSerializerCallback CHUNKED_ENCODING_BASE_SERIALIZER = GetBaseSerializer(false);
    private static readonly ResponseSerializerCallback BUFFERING_BASE_SERIALIZER = GetBaseSerializer(true);

    /// <summary>
    ///     Sends data in a HTTP response without serialization.
    /// </summary>
    /// <param name="bufferResponse">
    ///     <see langword="true" /> to write the response body to a memory buffer first,
    ///     then send it all together with a <c>Content-Length</c> header; <see langword="false" /> to use chunked
    ///     transfer encoding.
    /// </param>
    /// <returns>A <see cref="ResponseSerializerCallback" /> that can be used to serialize data to a HTTP response.</returns>
    /// <remarks>
    ///     <para>
    ///         <see cref="string" />s and one-dimensional arrays of <see cref="byte" />s
    ///         are sent to the client unchanged; every other type is converted to a string.
    ///     </para>
    ///     <para>
    ///         The <see cref="IHttpResponse.ContentType">ContentType</see> set on the response is used to negotiate
    ///         a compression method, according to request headers.
    ///     </para>
    ///     <para>
    ///         Strings (and other types converted to strings) are sent with the encoding specified by
    ///         <see cref="IHttpResponse.ContentEncoding" />.
    ///     </para>
    /// </remarks>
    public static ResponseSerializerCallback None(bool bufferResponse)
    {
        return bufferResponse ? BUFFERING_BASE_SERIALIZER : CHUNKED_ENCODING_BASE_SERIALIZER;
    }

    private static ResponseSerializerCallback GetBaseSerializer(bool bufferResponse)
    {
        return async (context, data) =>
        {
            if (data is null)
            {
                return;
            }

            var isBinaryResponse = data is byte[];

            if (!context.TryDetermineCompression(context.Response.ContentType, out var preferCompression))
            {
                preferCompression = true;
            }

            preferCompression = false;

            if (isBinaryResponse)
            {
                var responseBytes = (byte[])data;
                using var stream = context.OpenResponseStream(bufferResponse, preferCompression);
                await stream.WriteAsync(responseBytes).ConfigureAwait(false);
            }
            else
            {
                var responseString = data is string stringData ? stringData : data.ToString() ?? string.Empty;
                await using var text = context.OpenResponseText(context.Response.ContentEncoding, bufferResponse,
                    preferCompression);
                await text.WriteAsync(responseString).ConfigureAwait(false);
            }
        };
    }
}