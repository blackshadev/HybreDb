import matplotlib.pyplot as plt
import json
import sys

def openResults(fname):
	with open(fname) as dataFile:
		data = json.load(dataFile)
	return data

def plotResult(res):
	X = []
	Y = []
	y_err = []

	for k in res:
		X.append(int(k))
		Y.append(res[k][0])
		y_err.append(res[k][1])
	s = sorted(zip(X, Y, y_err))
	return zip(*s)

def main():
	files = [("postgres", "results/pgInsBench_2.json"), ("mysql", "results/mysqlInsBench_2.json"), ("HybreDb","results/hybreInsBench_2.json")]

	for (lbl, fname) in files:
		res = openResults(fname)
		X, Y, y_err = plotResult(res)

		plt.errorbar(X, Y, yerr=y_err, label=lbl)

	plt.xscale('log')

	plt.legend(loc=2)
	plt.savefig("plot.png")


if __name__ == '__main__':
	main()