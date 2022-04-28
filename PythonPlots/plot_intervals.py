import matplotlib.pyplot as plt
import numpy as np
import pandas as pd
from collections import Counter
import time
 # Modul sys wird importiert:
import sys

f = open("C:/Users/LFU/Desktop/tmp/intervals.csv", "r")

plt.figure()
lines = f.readlines()
intervals = eval(lines[0])
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
        elif(len(row)==2):
            plt.plot(x,[row_index,row_index],"y")
        elif(len(row)==3):
            plt.plot(x,[row_index,row_index], "g")
        else:
            plt.plot(x,[row_index,row_index], c=(0.9,0.9,0.9))
    

for row_index, row in enumerate(intervals):
    if(len(row)==1):
        plt.plot(row[0],[row_index,row_index],"r" ,label = "ein-teilig")
        break

for row_index, row in enumerate(intervals):
    if(len(row)==2):
        plt.plot(row[0],[row_index,row_index],"y" ,label = "zwei-teilig")
        break
for row_index, row in enumerate(intervals):
    if(len(row)==3):
        plt.plot(row[0],[row_index,row_index],"g" ,label = "drei-teilig")
        break

plt.legend(loc =2)

#plt.plot([0,0],[0,0],"y" ,label = "zwei-teilig")
#plt.plot([0,0],[0,0],"g" ,label = "drei-teilig")
time = [0,6,12,18,24]
plt.xticks([x*60*60 for x in time],time)
#plt.legend()
for line in lines[1:]:
    eval(line)
plt.show()
