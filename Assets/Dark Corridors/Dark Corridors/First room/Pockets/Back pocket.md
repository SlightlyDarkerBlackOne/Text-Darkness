"id": back_pocket

"text_variations":
Empty back pocket, Check back pocket, Inspect back pocket,

"result":

"Your back pocket seems empty, but as you reach in, you find a ==folded up piece of paper==." 

"“I can’t remember what this is. And it’s too dark to tell exactly.”"

"event": If not introduced, introduce inventory mechanic

"requirements": get_up

////////////////////////////////////////////////////////////////////////////////////////////////////////////////

"result":
""I already did that. There was a ==folded piece of paper==, but I can't tell what it is in the dark.""

"requirements": back_pocket or empty_pockets

[[Folded piece of paper]]

////////////////////////////////////////////////////////////////////////////////////////////////////////////////

"result":

""I already did that. There was this **==folded piece of paper==** with a weird **==drawing==**. I don't think I drew that.""

"requirements": franks_light_illuminates_paper
