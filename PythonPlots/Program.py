import matplotlib.pyplot as plt
import numpy as np
import pandas as pd

data = pd.read_csv("C:/Users/LFU/Desktop/Masterarbeit/Algorithm_data/test.csv",sep=";",index_col=0)
print(data)

fig, axs = plt.subplots(2,len(data.columns))
data = data.astype(float)
fig.tight_layout()
for i, attribute in enumerate(data.columns):
    axs[0][i].set_xticks(np.arange(len(data)))
    data[attribute].plot(legend=True, ax=axs[0][i])
    axs[0][i].tick_params(axis='x', labelrotation= 90)
axs[1][0].axis('off')
axs[1][1].axis('off')
plt.show()
