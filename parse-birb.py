#!/bin/python3

import wikitextparser as wtp
import requests
import json

r = requests.get('https://de.wikipedia.org/w/index.php?title=Liste_der_V%C3%B6gel_Deutschlands&action=raw')
wt = wtp.parse(r.text)

birbarr = []

tt = dict.fromkeys(map(ord, "[]'"), None)

for table in wt.tables:
    size = len(table.data())
    for i in range(3,size):
        ger = table.data()[i][1].strip().translate(tt)
        idx = ger.find("|")
        if idx:
            ger = ger[idx+1:]
        
        lat = table.data()[i][2].strip().translate(tt)
        idx = lat.find("|")
        if idx:
            lat = lat[idx+1:]
        
        
        birbarr += [{ "de": ger, "latin": lat }]
    

birbjson = json.dumps(birbarr,ensure_ascii=False).encode('utf8')

birbfile = open('birb.json', 'w')
birbfile.write(birbjson.decode())
birbfile.flush()
birbfile.close()