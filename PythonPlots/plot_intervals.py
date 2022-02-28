import matplotlib.pyplot as plt
import numpy as np
import pandas as pd
from collections import Counter
import time
 # Modul sys wird importiert:
import sys

f = open("C:/Users/LFU/Desktop/tmp/intervals.csv", "r")

intervals = eval(f.read())
plt.figure()
plt.xlabel("Uhrzeit")
plt.ylabel("Dienste")
for row_index, row in enumerate(intervals):
    #for x in row:
    #    plt.plot(x,[row_index,row_index])
    #if(len(row)==1):
    #    plt.plot([10600,10700],[row_index,row_index], "r")
    #if(len(row)==2):
    #    plt.plot([10600,10700],[row_index,row_index], "y")
    #if(len(row)==3):
    #    plt.plot([10600,10700],[row_index,row_index], "g")
    for x in row:
        if(len(row)==1):
            plt.plot(x,[row_index,row_index], "r")
        if(len(row)==2):
            plt.plot(x,[row_index,row_index],"y")
        if(len(row)==3):
            plt.plot(x,[row_index,row_index], "g")


plt.plot([10000,10000],[1,1],"r" ,label = "ein-teilig")
plt.plot([10000,10000],[1,1],"y" ,label = "zwei-teilig")
plt.plot([10000,10000],[1,1],"g" ,label = "drei-teilig")
time = [0,6,12,18,24]
plt.xticks([x*60*60 for x in time],time)
plt.legend()
plt.show()
