import matplotlib.pyplot as plt
import time

data = open("populationLog.txt","r")
groundx = [0,1000,1500,3000,4000,5500,6999]
groundy = [100,500,1500,1000,150,150,800]
for line in data.readlines():
    print(line)
    x = []
    y = []
    if "gen" in line:
        [gen,score,*rest]=line.split(";")
        plt.title(f"gen: {gen} score: {score}")
        plt.plot(groundx,groundy,"-b")
        plt.show(block=False)
        input()
        plt.close()

    else:
        for pair in line.split(";"):
            [px,py] = pair.split(",")
            if "score" not in px:
                x.append(float(px))
                y.append(float(py))
    print(x,y)
    plt.plot(x,y,"-")
