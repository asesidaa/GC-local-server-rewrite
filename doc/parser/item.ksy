meta:
  id: item_dat
  ks-debug: false
  ks-opaque-types: false
  endian: be
  encoding: UTF-8
seq:
  - id: count
    type: u2
  - id: items
    type: item
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
  item:
    seq:
      - id: item_id
        type: u4
      - id: id_str
        type: str_with_len
      - id: name
        type: str_with_len
      - id: unknown_bool
        type: u1
      - id: item_tooltip_jp
        type: str_with_len
      - id: item_tooltip_en
        type: str_with_len 
  