import matplotlib.pyplot as plt
import numpy as np
import pandas as pd
from collections import Counter
import time
 # Modul sys wird importiert:
import sys

f = open("C:/Users/LFU/Desktop/tmp/string_to_execute.txt", "r")

for line in f.readlines():
    eval(line)
