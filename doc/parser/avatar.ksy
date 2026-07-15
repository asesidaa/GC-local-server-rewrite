meta:
  id: avatar_dat
  ks-debug: false
  ks-opaque-types: false
  endian: be
  encoding: UTF-8
seq:
  - id: count
    type: u2
  - id: avatars
    type: avatar
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
  names_entry:
    seq:
      - id: full_name
        type: str_with_len
      - id: name
        type: str_with_len
      - id: variant
        type: str_with_len
  avatar:
    seq:
      - id: id
        type: u4
      - id: id_str
        type: str_with_len
      - id: name_entry_jp
        type: names_entry
      - id: name_entry_en
        type: names_entry
      - id: unknwon_byte1
        type: u1
      - id: png_file_name
        type: str_with_len
      - id: uvb_file_name
        type: str_with_len
      - id: efcb2_file_name
        type: str_with_len
      - id: unknown_int1
        type: u4
      - id: unknown_float1
        type: u1
      - id: unknown_float2
        type: u1
      - id: unknown_float3
        type: u1
      - id: unknown_float4
        type: u1
      - id: unknown_byte2
        type: u1
      - id: unknown_int2
        type: u4
      - id: unknown_int3
        type: u4
      - id: unknown_int4
        type: u4
      - id: unknown_int5
        type: u4
      - id: unknown_byte3
        type: u1
      - id: acquire_method_jp
        type: str_with_len
      - id: acquire_method_en
        type: str_with_len
      - id: unknown_int6
        type: u4