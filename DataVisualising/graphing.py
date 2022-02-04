import numpy as np
import os
import plotly as py
import matplotlib.pyplot as plt
import seaborn as sns
from flask import Flask, request
app = Flask(__name__)

@app.route('/')
def hello_world():
    return 'Hello, World!'

@app.route('/graph', methods=['POST'])
def graph():
    request_data = request.get_json()
    for i in range(len(request_data)):
        evolving_index = request_data[i]

        for exchange in evolving_index:
            averageSimulationData = calculateAverageSimulation(evolving_index[exchange])
            days = []

            #Number of days to array for graphing
            for j in range(len(evolving_index[exchange][0])):
                days.append(j)

            visualiseEndOfDaySatisfactions(i, exchange, days, averageSimulationData)
        #heatmap goes here
        visualiseHeatMaps(i, evolving_index)

    #print(np.matrix(averageSimulationData))     #Printing to check calculations work as expected
    return "Data visualisation complete!"

#Function that will graph the end of day average satisfactions for agent types in the model
#Optimal - The highest satisfaction rate agents could achieve if desired were fulfilled as best as could be
#Random - The average satisfaction of agents at the beginning of each day with the slots they were randomly allocated
#Selfish - The average satisfaction of selfish agents
#Social - The average satisfaction of social agents
def visualiseEndOfDaySatisfactions(evolveId, exchange, days, simDat):
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
    
    errorBarsDays = []

    selfishPosError = []
    socialPosError = []

    selfishNegError = []
    socialNegError = []

    socialSpecificDays = []
    selfishSpecificDays = []

    #Also need to add in one for the last day (since this only goes to 149)
    #So check that if day+1 == len(days) and day % 50 then add to errorBarsDays
    for day in days:
        if(day > 0 and day % 50 == 0):
            errorBarsDays.append(day)

    # Adding in last day error bar
    # if (days[len(days)-1] + 1) % 50 == 0:
    #     print("good")
    #     errorBarsDays.append(days[len(days)-1])

    for day in errorBarsDays:
        #Selfish error bars
        selfishSD = simDat[day][3]

        if selfishSD + selfish[day] >= 1:
            selfishPosError.append(1 - selfish[day])
        else:
            selfishPosError.append(selfishSD)
        
        if selfish[day] - selfishSD <= 0:
            selfishNegError.append(selfish[day])
        else:
            selfishNegError.append(selfishSD)

        selfishSpecificDays.append(selfish[day])

        #Social error bars
        socialSD = simDat[day][5]

        if socialSD + social[day] >= 1:
            socialPosError.append(1 - social[day])
        else:
            socialPosError.append(socialSD)
        
        if social[day] - socialSD <= 0:
            socialNegError.append(social[day])
        else:
            socialNegError.append(socialSD)

        socialSpecificDays.append(social[day])




    fig = py.graph_objects.Figure(layout_yaxis_range=[0,1])

    fig.add_trace(py.graph_objects.Scatter(
        x=days,
        y=selfish,
        mode='lines',
        name='Selfish',
        line=dict(
            color='blue'
        )
    ))

    fig.add_trace(py.graph_objects.Scatter(
        x=days,
        y=social,
        mode='lines',
        name='Social',
        line=dict(
            color='red'
        )
    ))

    fig.add_trace(py.graph_objects.Scatter(
        x=days,
        y=optimal,
        mode='lines',
        name='Optimal',
        line=dict(
            color='green'
        )
    ))

    fig.add_trace(py.graph_objects.Scatter(
        x=days,
        y=random,
        mode='lines',
        name='Random',
        line=dict(
            color='magenta'
        )
    ))

    fig.add_trace(py.graph_objects.Scatter(
        x=errorBarsDays,
        y=socialSpecificDays,
        mode='markers',
        marker=dict(
            color='red'
        ),
        showlegend=False,
        error_y=dict(
            type='data',
            symmetric=False,
            array=socialPosError,
            arrayminus=socialNegError
        )
    ))

    fig.add_trace(py.graph_objects.Scatter(
        x=errorBarsDays,
        y=selfishSpecificDays,
        mode='markers',
        marker=dict(
            color='blue'
        ),
        showlegend=False,
        error_y=dict(
            type='data',
            symmetric=False,
            array=selfishPosError,
            arrayminus=selfishNegError
        )
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

    fig.write_image("./avg_"+str(evolveId)+"_"+str(exchange)+".pdf")

#Function that will create heat maps of the models to visualise how the number of exchange rounds effects satisfactions
def visualiseHeatMaps(evolveId, evolving_index):
    fig, axs = plt.subplots(nrows=2, ncols=1, figsize=(8,6))
    plt.subplots_adjust(hspace=0.8)
    fig.suptitle("Learning 100%", fontsize=16)

    exchangeAllocationData = calculateGetHeatMapData(evolving_index)

    #extracting selfish and social satisfaction for each allotment of exchange rounds
    selfishSatisfaction = []
    socialSatisfaction = []

    #For loop that will go over each of the exchange allocation models and will loop over all the days in that model
    #So that data for days in that model can be extracted
    for exchangeAllocation in exchangeAllocationData:
        extractSocialSatisfactions = []
        extractSelfishSatisfactions = []
        for day in exchangeAllocation:
            extractSocialSatisfactions.append(round(day[4],2))
            extractSelfishSatisfactions.append(round(day[2],2))
        socialSatisfaction.append(extractSocialSatisfactions)
        selfishSatisfaction.append(extractSelfishSatisfactions)

    row_labels=['1', '100', '200', '300', '400', '500']
    col_labels=['1', '50', '100', '150', '200']

    DOIsocial = [[0 for x in range(len(col_labels))] for y in range(len(row_labels))]
    DOIselfish = [[0 for x in range(len(col_labels))] for y in range(len(row_labels))]

    for i in range(len(col_labels)):
        for j in range(len(row_labels)):
            DOIsocial[j][i] = socialSatisfaction[i][int(row_labels[j])-1]
            DOIselfish[j][i] = selfishSatisfaction[i][int(row_labels[j])-1]

    socialheatmap = sns.heatmap(DOIsocial, ax=axs[0], yticklabels=row_labels, xticklabels=col_labels, annot=True)
    axs[0].set_title('Average Social Satisfaction')
    axs[0].set(xlabel='Exchanges', ylabel='Days')
    socialheatmap.invert_yaxis()

    selfishheatmap = sns.heatmap(DOIselfish, ax=axs[1], yticklabels=row_labels, xticklabels=col_labels, annot=True)
    axs[1].set_title('Average Selfish Satisfaction')
    axs[1].set(xlabel='Exchanges', ylabel='Days')
    #socialheatmap.set_title('Average Selfish Satisfaction')
    selfishheatmap.invert_yaxis()


    #plt.tight_layout()
    fileName = "./heatmap_"+str(evolveId)+".pdf"
    plt.savefig(fileName,
        dpi=None,
        facecolor='w',
        edgecolor='w',
        orientation='landscape',
        format=None,
        transparent=False,
        bbox_inches=None,
        pad_inches=0,
        metadata=None)

#Function to go over the data for a running of the model and get the average values for each day over each repeated run
def calculateAverageSimulation(data):
    numOfDays = len(data[0])
    dayEntries = len(data[0][0])

    #Array that will store the average data for each day over all simulations that were run
    dayAverages = [[0 for x in range(dayEntries)] for y in range(numOfDays)]

    #For every simulation - Add each day and the data for that day to the dayAverages array
    #Currently only doing this for 1 of the evolving agent model runs - add another loop above this to go over each evolving agent model as well?
    for simulation in range(len(data)):
        for i in range(len(data[simulation])):
            #for each day
            for j in range(len(data[simulation][i])):
                #For day data
                dayAverages[i][j] += data[simulation][i][j]
                # selfish.append(simulation[i][2])
                # social.append(simulation[i][4])
    
    #Getting the averages for days in the simulation
    for i in range(len(dayAverages)):
        for j in range(len(dayAverages[i])):
            dayAverages[i][j] /= len(data)

    return dayAverages

#Function that will extract information for heat map - will have to extract data from each of the different number of allocated
#exchange rounds
#Returns an array of arrays - an array of averages of each of the allocated exchange rounds
def calculateGetHeatMapData(data):
    arrayOfExchangeAllocationData = []
    for exchange in data:
        averageDataForExchangeAllocation = calculateAverageSimulation(data[exchange])
        arrayOfExchangeAllocationData.append(averageDataForExchangeAllocation)
    return arrayOfExchangeAllocationData

if __name__ == '__main__':
    app.run(debug=True, port=5000)