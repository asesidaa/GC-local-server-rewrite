meta:
  id: navigator_dat
  ks-debug: false
  ks-opaque-types: false
  endian: be
  encoding: UTF-8
seq:
  - id: count
    type: u2
  - id: navigators
    type: navigator
    repeat: expr
    repeat-expr: count
types:
  str_with_len:
    seq:
      - id: len
        type: u1
      - id: value
        type: str
        encoding: UTF-8
        size: len
  name_entries:
    seq:
      - id: name_with_variant
        type: str_with_len
      - id: name_without_variant
        type: str_with_len
      - id: variant
        type: str_with_len
      - id: illustration_credit
        type: str_with_len
  navigator:
    seq:
      - id: id
        type: u4
      - id: identifier
        type: str_with_len
      - id: file_name
        type: str_with_len
      - id: name_entries0
        type: name_entries
      - id: name_entries1
        type: name_entries
      - id: genre
        type: u1
        enum: genre
      - id: unknown_enum0
        type: u1
      - id: unknown_bool0
        type: u1
      - id: unknown_bool1
        type: u1
      - id: unknown_bool2
        type: u1
      - id: zeros
        size: 7
      - id: default_availability
        type: u1
        enum: default_availability
      - id: unknown_enum1
        type: u1
      - id: unknown_bool3
        type: u1 
      - id: navigator_tooltip_jp
        type: str_with_len
      - id: navigator_tooltip_en
        type: str_with_len
enums:
  genre:
    1: default
    2: original
    3: game
    4: touhou
    5: vocaloid
    6: collab
  default_availability:
    0: not_available
    1: available
    2: available_with_voice
      