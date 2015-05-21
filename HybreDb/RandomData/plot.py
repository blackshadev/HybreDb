import matplotlib.pyplot as plt
import json
import sys
import numpy as np

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
		Y.append(np.mean(res[k]))
		y_err.append(np.std(res[k], ddof=1))
	s = sorted(zip(X, Y, y_err))
	return zip(*s)

def main():
	f = sys.argv[1]
	t = "BulkInsert performance"
	if(len(sys.argv) > 2):
		t = sys.argv[2]

	files = [
		("Postgres", "results/" + f + "/Postgres.json"), 
		("MySQL", "results/" + f + "/MySQL.json"), 
		("HybreDb","results/" + f + "/HybreDb.json")
	]

	for (lbl, fname) in files:
		res = openResults(fname)
		X, Y, y_err = plotResult(res)

		plt.errorbar(X, Y, yerr=y_err, label=lbl)

	plt.xlabel("Dataset in rijen")
	plt.ylabel("Tijd in ms")

	plt.title(t)

	plt.xscale('log')

	plt.legend(loc=2)
	plt.savefig("results/" + f + "/plot.png")


if __name__ == '__main__':
	main()