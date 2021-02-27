#!/bin/python3

import wikitextparser as wtp
import requests
import json

r = requests.get('https://de.wikipedia.org/w/index.php?title=Liste_der_V%C3%B6gel_Deutschlands&action=raw')
wt = wtp.parse(r.text)

birbarr = []

for table in wt.tables:
    size = len(table.data())
    for i in range(3,size):
        birbarr += [{ "de": table.data()[i][1][2:-2], "latin": table.data()[i][2][2:-2] }]
    

birbjson = json.dumps(birbarr,ensure_ascii=False).encode('utf8')

birbfile = open('birb.json', 'w')
birbfile.write(birbjson.decode())
birbfile.flush()
birbfile.close()