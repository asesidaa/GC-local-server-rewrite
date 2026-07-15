meta:
  id: title_dat
  ks-debug: false
  ks-opaque-types: false
  endian: be
  encoding: UTF-8
seq:
  - id: count
    type: u2
  - id: titles
    type: title
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
  title:
    seq:
      - id: title_id
        type: u4
      - id: id_str
        type: str_with_len
      - id: file_suffix
        type: str_with_len
      - id: name_jp
        type: str_with_len
      - id: name_eng
        type: str_with_len
      - id: enabled
        type: u1
      - id: unlock_requirement_jp
        type: str_with_len
      - id: unlock_requirement_en
        type: str_with_len 
      - id: next_unlock_requirement_jp
        type: str_with_len
      - id: next_unlock_requirement_en
        type: str_with_len 
      - id: title_type
        enum: unlock_type
        type: u1
      - id: unknown0
        type: u1
      - id: unknown1
        type: u1
      - id: related_song_id
        type: u2
      - id: title_chain_id
        type: u1
enums:
  unlock_type:
    0: invalid
    1: default
    2: clear
    3: no_miss
    4: full_chain
    5: s_rank_simple_stages
    6: s_rank_normal_stages
    7: s_rank_hard_stages
    8: s_rank_extra_stages
    9: s_rank_s_n_h
    10: s_plus_rank_s_n_h
    11: s_plus_plus_rank_s_n_h
    12: event
    13: prefecture
    14: chain_milestone
    15: adlibs
    16: consequtive_no_miss
    17: clears_using_items
    18: avatars
    19: multiplayer_stars_total
    20: song_set_20
    21: song_set_21
    22: song_set_22
    23: song_set_23
    24: song_set_24
    25: song_set_25
    26: song_set_26
    27: profile_level
    28: perfect
    29: online_matching
    30: trophies
   