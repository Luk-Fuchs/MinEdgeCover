import matplotlib.pyplot as plt
import numpy as np
import pandas as pd
from collections import Counter
import time
 # Modul sys wird importiert:
import sys

f = open("C:/Users/LFU/Desktop/tmp/values.csv", "r")
splitted = f.readline().split(";")  
x = [float(_) for _ in splitted[0].split(",")]
y = [float(_) for _ in splitted[1].split(",")]

dict = {}
f = open("C:/Users/LFU/Desktop/tmp/parameters.csv", "r")
splitted = f.readline().split(";")
for _ in splitted:
    pair = _.split(",")
    dict[pair[0]] = pair[1]


fig = plt.figure(figsize=(12, 6))
if dict["plottype"]=="bar":
    plt.bar(x,y)
else:
    plt.plot(x,y)
for horizontal in dict["horizontal"].split("|"):
    if horizontal=="":
        break
    horizontal = float(horizontal)
    plt.plot([min(x),max(x)],[horizontal,horizontal],"r--")
for vertical in dict["vertical"].split("|"):
    if vertical=="":
        break
    vertical = float(vertical)
    plt.plot([vertical,vertical],[min(y),max(y)],"r--")
plt.title(dict["title"])
plt.xlabel(dict["xLabel"])
plt.ylabel(dict["yLabel"])
plt.savefig("C:/Users/LFU/Desktop/tmp/fig" + str(time.time()) + ".pdf", dpi=fig.dpi)

if(dict["logarithmic"]=="True"):
    plt.semilogy()


plt.ylim([0,max(y)*1.1])

if dict["show"]=="True":
    plt.show()



#x = f.readline()
#y = f.readline()
#x = f.readline()

