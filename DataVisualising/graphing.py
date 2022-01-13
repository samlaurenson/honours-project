import numpy as np
from flask import Flask, request
app = Flask(__name__)

@app.route('/')
def hello_world():
    return 'Hello, World!'

@app.route('/graph', methods=['POST'])
def graph():
    request_data = request.get_json()
    evolving_index = request_data[0]
    simulation = evolving_index['Value'][0]
    day = simulation[0][0]

    averageSimulationData = calculateAverageSimulation(evolving_index) #doing this with just the first evolving array as trial - will need to loop this for each evolving agent
    print(np.matrix(averageSimulationData))     #Printing to check calculations work as expected
    return "The average agent satisfaction for 1st day of 1st simulation is " + str(day)

def calculateAverageSimulation(data):
    numOfDays = len(data['Value'][0])
    dayEntries = len(data['Value'][0][0])

    #Array that will store the average data for each day over all simulations that were run
    dayAverages = [[0 for x in range(dayEntries)] for y in range(numOfDays)]

    #For every simulation - Add each day and the data for that day to the dayAverages array
    for simulation in data['Value']:
        for i in range(len(simulation)):
            #for each day
            for j in range(len(simulation[i])):
                #For day data
                dayAverages[i][j] += simulation[i][j]
    
    #Getting the averages for days in the simulation
    for i in range(len(dayAverages)):
        for j in range(len(dayAverages[i])):
            dayAverages[i][j] /= len(data['Value'])

    return dayAverages





if __name__ == '__main__':
    app.run(debug=True, port=5000)