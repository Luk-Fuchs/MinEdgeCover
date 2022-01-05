import matplotlib.pyplot as plt
import numpy as np
import pandas as pd
from collections import Counter

data = pd.read_csv("C:/Users/LFU/Desktop/Masterarbeit/Algorithm_data/test.csv",sep=";",index_col=0)
print(data)

fig, axs = plt.subplots(2,len(data.columns[:-2]))
data = data.astype(float)
fig.tight_layout()
for i, attribute in enumerate(data.columns[:-2]):
    axs[0][i].set_xticks(np.arange(len(data)))
    #data[attribute].plot(legend=True, ax=axs[0][i])
    axs[0][i].bar(data.index,data[attribute])
    axs[0][i].tick_params(axis='x', labelrotation= 90)
    axs[1][i].axis('off')
#axs[1][1].axis('off')
plt.show()



## Verteilung der Knotengrade:
#degrees = [4,4,9,10,3,2,1,9,4,1,7,47,34,30,24,1,1,32,1,55,12,26,25,61,46,43,52,37,1,41,49,1,49,1,1,56,46,55,31,18,9,72,1,53,15,24,16,11,1,45,69,1,1,34,1,1,1,11,24,12,1,3,1,24,36,19,49,14,37,29,66,70,63,34,1,43,112,89,86,87,126,155,92,48,54,3,135,58,164,7,160,36,54,138,107,184,157,97,78,41,36,206,294,200,113,141,111,35,128,1,58,19,42,68,30,14,24,113,30,8,31,43,223,18,21,64,44,142,19,32,23,154,75,28,108,27,82,34,21,279,7,82,21,315,48,1,53,335,37,1,50,1,19,2,133,40,70,55,1,38,37,32,62,71,85,22,64,1,31,150,171,1,118,106,73,33,7,117,250,1,7,4,158,112,107,318,184,243,1,28,6,25,272,3,89,21,92,46,29,66,13,69,1,40,3,23,1,9,49,17,31,97,74,77,104,109,50,64,145,18,34,41,30,115,186,9,34,2,164,59,90,76,65,79,57,162,85,71,121,114,188,55,42,25,12,4,37,13,54,49,56,126,123,13,37,13,21,76,25,2,25,105,62,30,1,1,25,50,13,42,29,48,3,29,1,112,10,2,2,11,141,9,14,64,45,18,12,69,8,6,185,38,101,13,109,3,46,4,3,5,27,15,15,2,30,6,37,7,13,3,7,15,18,8,7,18,1,1,6,8,4]
#x =Counter(degrees)
#plt.figure()
#plt.bar(x.keys(), x.values())
#plt.show()