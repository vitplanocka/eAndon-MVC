# eAndon

Implementation of a simple electronic Andon system designed to facilitate communication and visualization of issues occurring at workstations or production lines in a production company. 

By configuring the app, various types of problems such as technical issues, quality issues, and material supply issues can be displayed, and it can also be tailored to display different workstation configurations and display screens. 

This system can be accessed from a web browser or mobile device, and it can be run on an intranet or internet connection. T

<img src="https://github.com/vitplanocka/eAndon-MVC/blob/master/eAndon%20MVC/wwwroot/Images/Overview.png" alt="Overview and terminal application displaying triggered alarms" width="800">

* [What is Andon?](#What-is-Andon)

* [Features and limitations](#Features-and-limitations)

* [Installation](#Installation)

* [Instructions](#Instructions)

* [License](#License)

* [Author](#Author)


## What is Andon?
Andon is a popular tool used in Lean Manufacturing. It was originally pioneered by Toyota as a method to visualize problems, help create employee's awareness about targets and non-standard conditions, and promote leadership behavior. It is linked with the Jidōka methodology in the Toyota Production system that encourages operators to recognize a deviation from the standard and stop work and call supervisors or support staff who can help solve the deviation.  Most Japanese factories display a variation on the sign「止める・呼ぶ・待つ」- Stop-Call-Wait that reminds operators that this is the expected behavior.

<img src="https://github.com/vitplanocka/eAndon-MVC/blob/master/eAndon%20MVC/wwwroot/Images/pkmjerei.jpeg" alt="Stop-Call-Wait sign"  width="300">

Originally, the operator would pull the Andon Cord, which was a rope located above the line, but Andon can take many forms. It can be activated by an operator pulling a cord or pushing a button, or it can be automatically activated by equipment when a problem is detected.

<img src="https://github.com/vitplanocka/eAndon/blob/master/Screenshots/Andon_cord2.png" alt="Operator pulling an Andon Cord"  width="400">

Whether used because of part shortage, equipment malfunction, or a safety concern, the point of Andon in Lean manufacturing is to stop work so that the team can gather together, perform a real-time root cause analysis, and quickly apply a solution. Once the problem is resolved and work continues, the occurrence is logged as part of a continuous improvement system.

<b>Electronic/software Andon</b>

Most modern production facilities implement some kind of computer terminals on the shop floor to record the production data, allow the operators to access the working instructions, etc. Such terminals can also be used effectively to run an Andon software so that it is not necessary to use physical systems (e.g. an Andon Cord or specialized industrial solutions consisting of custom electronics with buttons and visual displays). The advantage of the electronic/software system is the cost (in case terminals and display screens already exist on the shop floor, they can be used to run also the software Andon) and flexibility - new types of alarms or new workstations can be added quickly without additional costs.

<b>Diagram of the eAndon</b>

The illustration below shows a typical setup where there are several networked terminals (each running a separate Andon terminal application), each terminal handles one or more workstations.
There is one or more dashboards (TV display in a production hall, laptop computer used by factory staff, mobile phone …) that display the total shop floor overview.

<img src="https://github.com/vitplanocka/eAndon-MVC/blob/master/eAndon%20MVC/wwwroot/Images/Andon_diagram.png" alt="Diagram of Andon_dashboard and terminals" width="700">


## Features

*	Allows operator to trigger a number of different alarms (the app allows from 1 – 5 alarm types)
*	Alarms are displayed on one or more dashboard which can be placed in communal areas (e.g. TV placed on the shopfloor visible to all) or dedicated team areas (e.g. maintenance team’s workshop) as well as displayed on laptops or mobile phones
*	Alarms are displayed in a way to give to Production management a quick overview about the status of the production process
*	With every alarm, the duration of the alarm is displayed and logged, creating a psychological nudge to solve the abnormal situation as soon as possible
* Clicking the workstation number opens a window showing the history of alarms and their durations
*	The number and names of terminals (workstations) can be configured freely
*	Labels and text in the app can be modified and easily localized to another language



## Installation

The app stores the settings and alarm data in an SQL database.
In order to set up the SQL tables, you can use the scripts from the <a href="https://github.com/vitplanocka/eAndon-MVC/blob/master/eAndon%20MVC/SQL_tables.txt">SQL_tables<a> file. 

In order to customize the setup for the specific use case, access the Settings page in the app that allows you the following customization:
*     Adding, removing, or renaming terminals to accurately represent your existing workstations
*     Creating, deleting, or modifying alarm types, along with specifying if additional details should be provided for each alarm
*     Adjusting the app's text content or translating it to another language for localization purposes


## Instructions

When a problem occurs at a workstation, click the green field corresponding to the incident in the relevant Terminal screen. If specific details are available for the alarm, you can input additional information, such as the location of the failure, the type of failure, or any other pertinent details in the form of free text. Once entered, the green field will change to red, and a timer will begin to count the seconds or minutes since the alarm was activated.

This information will also be immediately reflected on the Overview visualization.

After resolving the alarm at the workstation, click the red field in the Terminal to reset the alarm status back to green.

<img src="https://github.com/vitplanocka/eAndon-MVC/blob/master/eAndon%20MVC/wwwroot/Images/Overview.png" alt="Overview and terminal application displaying triggered alarms" width="800"> 

Logs of triggered alarms can be found in the Logs tab, while Pareto diagrams of the alarms are available in the Statistics page.

<img src="https://github.com/vitplanocka/eAndon-MVC/blob/master/eAndon%20MVC/wwwroot/Images/Statistics.png" alt="Alarm statistics" width="700" style="border: 2px solid black;" />


## License
This project is licensed under the <a href="https://github.com/vitplanocka/eAndon/blob/master/LICENSE">MIT license</a>

### Author

**Vit Planocka**

Email: planocka@gmail.com

Location: Prague, Czechia

GitHub: https://github.com/vitplanocka

LinkedIn: https://www.linkedin.com/in/vitplanocka/
