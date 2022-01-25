import numpy as np
import os
import plotly as py
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
    days = []
    #Number of days to array for graphing
    for i in range(len(evolving_index['Value'][0])):
        days.append(i)


    averageOptimum = averageOptimumSatis(evolving_index)

    print(days)
    #plotEODAverage(days, averageSimulationData)
    test(days, averageSimulationData, averageOptimum)
    print(np.matrix(averageSimulationData))     #Printing to check calculations work as expected
    return "The average agent satisfaction for 1st day of 1st simulation is " + str(day)

def test(days, simDat, averageOptimum):
    selfish = []
    social = []
    optimal = []
    random = []

    #Extracting data for selfish and social agent satisfactions from the average model data
    for i in range(len(simDat)):
        selfish.append(simDat[i][2])
        social.append(simDat[i][4])
        optimal.append(simDat[i][1])
        random.append(simDat[i][0])

    fig = py.graph_objects.Figure()

    fig.add_trace(py.graph_objects.Scatter(
        x=days,
        y=selfish,
        mode='lines',
        name='Selfish',
    ))

    fig.add_trace(py.graph_objects.Scatter(
        x=days,
        y=social,
        mode='lines',
        name='Social',
    ))

    fig.add_trace(py.graph_objects.Scatter(
        x=days,
        y=optimal,
        mode='lines',
        name='Optimal',
    ))

    fig.add_trace(py.graph_objects.Scatter(
        x=days,
        y=random,
        mode='lines',
        name='Random',
    ))

    fig.update_layout(
        xaxis=dict(
            title='Day'
        ),
        yaxis=dict(
            title='Average consumer satisfaction'
        )
    )

    fig.update_layout(
        title_text="Average consumer satisfaction at end of each day",
        title_x=0.5
    )

    fig.write_image("./avg3.pdf")

def plotEODAverage(days, endOfDaySatisfactions):
    satisf = []
    for i in range(len(endOfDaySatisfactions)):
        satisf.append(endOfDaySatisfactions[i][0])

    data = []
    data.append(
        py.graph_objs.Scatter(
            x = days,
            y = satisf,
        )
    )

    layout: any = dict(
        title=dict(
            text='Average consumer satisfaction at the end of each day',
            xanchor='center',
            x=0.5,
        ),
        xaxis=dict(
            title='Day',
            showline=True,
            linecolor='black',
            linewidth=1,
            gridcolor='rgb(225, 225, 225)',
            gridwidth=1,
            range=[days[0], days[-1]],
            tickmode='linear',
            tick0=0,
            dtick=100,
        ),
        yaxis=dict(
            title='Average consumer satisfaction',
            showline=True,
            linecolor='black',
            linewidth=1,
            gridcolor='rgb(225, 225, 225)',
            gridwidth=1,
            range=[0, 1],
            tickmode='linear',
            tick0=0,
            dtick=0.2,
        ),
        margin=dict(
            l=40,
            r=30,
            b=80,
            t=100,
        ),
        paper_bgcolor='rgb(255, 255, 255)',
        plot_bgcolor='rgb(255, 255, 255)',
        font=dict(
            size=19
        )
    )

    graph = dict(data=data, layout=layout)
    path = os.path.join('.', "avg.pdf")
    py.io.write_image(graph, path)


def calculateAverageSimulation(data):
    numOfDays = len(data['Value'][0])
    dayEntries = len(data['Value'][0][0])

    #Array that will store the average data for each day over all simulations that were run
    dayAverages = [[0 for x in range(dayEntries)] for y in range(numOfDays)]

    #For every simulation - Add each day and the data for that day to the dayAverages array
    #Currently only doing this for 1 of the evolving agent model runs - add another loop above this to go over each evolving agent model as well?
    for simulation in data['Value']:
        for i in range(len(simulation)):
            #for each day
            for j in range(len(simulation[i])):
                #For day data
                dayAverages[i][j] += simulation[i][j]
                # selfish.append(simulation[i][2])
                # social.append(simulation[i][4])
    
    #Getting the averages for days in the simulation
    for i in range(len(dayAverages)):
        for j in range(len(dayAverages[i])):
            dayAverages[i][j] /= len(data['Value'])

    return dayAverages

def averageOptimumSatis(data):
    optimumSatisfactions = []
    for simulation in data['Value']:
        for i in range(len(simulation)):
            #for each day
            for j in range(len(simulation[i])):
                #For day data
                optimumSatisfactions.append(simulation[i][1])
    return np.average(optimumSatisfactions)



if __name__ == '__main__':
    app.run(debug=True, port=5000)