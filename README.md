# Visual Scripting Environment for R and Data Science
![alt text](https://github.com/alaabenfatma/VisualSR/blob/master/Resources/splash.gif "SPLASH")

### Technical article -> https://www.codeproject.com/Articles/1239656/VisualSR

### 📉 Status
![AppVeyor](https://img.shields.io/appveyor/build/alaabenfatma/visualsr?style=plastic)
![GitHub issues](https://img.shields.io/github/issues/alaabenfatma/visualsr)
![GitHub](https://img.shields.io/github/license/alaabenfatma/visualsr)

### 🔧 Pre-requirments
<ul>
  <li>.NET Framework 4.5</li>
  <li><b>rscript</b> has to be reachable from the command-line : https://www.r-project.org/</il>
</ul>

### 🏗️ Build 

Each project in this repo (VisualSR, Nodes and demo) can be built using one of these two different approaches.
<ul>
  <li>Automatically : Using an IDE, such as Visual Studio or SharpDevelop.</li>
  <li>Semi-manually : You can use <b>MSBuild</b></li>
    
<pre class="brush: python">
  MSBuild.exe VisualSR\VisualSR.sln
  MSBuild.exe Nodes\Nodes.sln
  MSBuild.exe demo\demo.sln
</pre>
  
</ul> 

### 📚 Usage
This repo contains 3 different projects. Each one of them contains a .csproj file that can load the whole project.
<ul>
  <li>VisualSR : This is the core of the project. It contains all the tools, custom controls and utilities. A class library (.dll) will be generated once the project is built.</li>
  <li>Nodes : This is a project within which I have created many samples of how to create custom nodes. A class library (.dll) will be generated once the project is built.</li>
  <li>DEMO : This GUI will provide the user with a UX which can help demonstrate the capabilities of this project.</li>
</ul> 

### 💕 Contributions
The project is far from being perfect. Please do no hesitate to open issues, debug the code if needed or make pull requests!

#### Please note that the rest not a documentation, it is nothing but an *eye-catcher*. Refer to this article [CODEPROJECT](https://www.codeproject.com/Articles/1239656/VisualSR) for more details.




### NODES

![alt text](https://github.com/alaabenfatma/VisualSR/blob/master/Resources/nodes.png "SPLASH")
<hr/> 

### CONNECTORS

#### Diagram

![alt text](https://github.com/alaabenfatma/VisualSR/blob/master/Resources/connectors_diag.png "SPLASH")
<hr/> 

### Applications

![alt text](https://github.com/alaabenfatma/VisualSR/blob/master/Resources/Conn_full.gif "SPLASH")
<hr/> 

### Making connections

![alt text](https://github.com/alaabenfatma/VisualSR/blob/master/Resources/ob_link_simple.gif "SPLASH")

### Possibilities

![alt text](https://github.com/alaabenfatma/VisualSR/blob/master/Resources/ob_link_poss.gif "SPLASH")
<hr/> 

### Deleting connections

![alt text](https://github.com/alaabenfatma/VisualSR/blob/master/Resources/delete.gif "SPLASH")

<hr/> 

### Middle Man Algorithm

![alt text](https://github.com/alaabenfatma/VisualSR/blob/master/Resources/MM.gif "SPLASH")
<hr/> 


### SAMPLES

#### Math

![alt text](https://github.com/alaabenfatma/VisualSR/blob/master/Resources/v_formula.png "SPLASH")
![alt text](https://github.com/alaabenfatma/VisualSR/blob/master/Resources/v_form_graph.png "SPLASH")
<hr/> 


#### For loop

![alt text](https://github.com/alaabenfatma/VisualSR/blob/master/Resources/for.png "SPLASH")
![alt text](https://github.com/alaabenfatma/VisualSR/blob/master/Resources/for_console.png "SPLASH")
<hr/> 

#### Graph

![alt text](https://github.com/alaabenfatma/VisualSR/blob/master/Resources/code1.png "SPLASH")
![alt text](https://github.com/alaabenfatma/VisualSR/blob/master/Resources/code2.png "SPLASH")
<hr/> 

## More stuff you will find 

#### A performance gauge

![alt text](https://github.com/alaabenfatma/VisualSR/blob/master/Resources/Gauge_Perform.gif "SPLASH")
<hr/> 

#### Commenting zones

![alt text](https://github.com/alaabenfatma/VisualSR/blob/master/Resources/comment1.png "SPLASH")
<hr/> 

#### Variables list

![alt text](https://github.com/alaabenfatma/VisualSR/blob/master/Resources/var_drag_drop.gif "SPLASH")
<hr/> 

#### Altered Nodes Tree

![alt text](https://github.com/alaabenfatma/VisualSR/blob/master/Resources/Tree_Nodes_Altered.gif  "SPLASH")
<hr/> 
 

#### Search for nodes

![alt text](https://github.com/alaabenfatma/VisualSR/blob/master/Resources/GoNode.gif  "SPLASH")
<hr/> 
<hr/> 

# DEMO
![alt text](https://github.com/alaabenfatma/VisualSR/blob/master/Resources/demo.png  "SPLASH")
<hr/> 
