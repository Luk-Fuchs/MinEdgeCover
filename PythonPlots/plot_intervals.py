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
for row_index, row in enumerate(intervals):
    for x in row:
        plt.plot(x,[row_index,row_index])
plt.show()