$('body').append($("<div id='visualGraphNavigation' style='text-align:center;' width='"+width+"' height='"+height+"'></div>"))
var frequenciesByAuthor = _.reduce(detailedAuthors.conversationSummaries, 
    function(acc, v, k){
        acc[k] = v.conversationCount;
        return acc;
    }, {});
var detailsByAuthor = _.reduce(detailedAuthors.conversationSummaries,
    function(acc,v,k){
        acc[k] = _.reduce(v.conversations.listing, function(acc, v, k){
            acc[v.title] = v
            return acc;
        }, {})
        return acc;
    }, {});
var flatNodes = pv.dom(frequenciesByAuthor).nodes();
var clusteredFrequencies = cluster(frequenciesByAuthor,6, false);
var clusteredNodes = pv.dom(clusteredFrequencies).nodes();
var dotScale = pv.Scale.linear(_.min(_.values(frequenciesByAuthor)), _.max(_.values(frequenciesByAuthor))).range(20,1000)
var marginScale = pv.Scale.linear().range(-100,100)

function deepen(level,k,v){
    var obj = {}
    obj[k] = v
    return _.reduce(_.range(level),function(acc){
        var wrapper = {}
        wrapper[k] = acc
        return wrapper 
    },obj) 
}
function cluster(subject, maxClusters, smooth){
    var subjectArray = _.map(subject, function(v,k){
        return [k,v]
    }).sort(function(a,b){
        return a[1] - b[1]
    })
    var subjectValues = _.values(subject)
    var limit = _.max(subjectValues)
    var divisor = Math.floor(limit / maxClusters);
    if(smooth){
        var i = 0
        return _.reduce(subjectArray, function(acc,kv){
            var k = kv[0]
            var v = kv[1]
            var divergenceThreshold = 1
            var group = (i < subjectArray.length * divergenceThreshold)?
                Math.floor(i++ / divisor):
                Math.floor(v / divisor) + 2
            var adjustedGroup = _.min([maxClusters, group])
            return _.extend(acc, deepen(adjustedGroup,k,v))
        }, {})
    }
    return _.reduce(subject, function(acc, v, k){
        var group = Math.floor(v / divisor)
        var adjustedGroup = _.min([maxClusters, group + 2])
        var deepGroup = deepen(adjustedGroup,k,v)
        return _.extend(acc, deepGroup)
    }, {});
}

var Master = {
    panel:function(){
        return new pv.Panel()
            .canvas("visualGraphNavigation")
            .left(0)
            .top(0)
            .bottom(0)
            .right(0)
            .width(width)
            .height(height)
    }
}
var Authors = {
    colors:_.reduce(_.keys(detailedAuthors.conversationSummaries), function(acc,item,index){
        var availableColors = pv.Colors.category19().range();
        acc[item] = availableColors[index % availableColors.length]
        return acc;
    },{}),
    color:function(d){
        if(!d.nodeName) return "black"
        return Authors.colors[d.nodeName]
    },
    data:clusteredNodes,
    squares : function(){
        var graphRoot = Master.panel()
        var treemap = graphRoot.add(pv.Layout.Treemap)
        .def("active", function(){ return -1})
        .nodes(Authors.data);
        treemap.leaf.add(pv.Panel)
            .fillStyle(Authors.color)
            .strokeStyle("#fff")
            .lineWidth(1)
            .event("click",Conversations.packed)
        treemap.label.add(pv.Label);
        graphRoot.render();
    },
    wedges: function(){
        var graphRoot = Master.panel()
        var newChild = graphRoot.add(pv.Layout.Partition.Fill)
            .nodes(Authors.data)
            .size(function(d){
                return dotScale(d.nodeValue)})
            .order("descending")
            .orient("radial");
        newChild.node.add(pv.Wedge)
            .fillStyle(Authors.color)
            .strokeStyle("#fff")
            .lineWidth(1)
        newChild.label.add(pv.Label);
        graphRoot.render();
    },
    radial: function(){
        var graphRoot = Master.panel()
        var newChild = graphRoot.add(pv.Layout.Tree)
            .nodes(Authors.data)
            .orient("radial")
        newChild.link.add(pv.Line)
        newChild.node.add(pv.Dot)
            .fillStyle(Authors.color)
            .size(function(d){
                return dotScale(d.nodeValue)})
            .event("click",Conversations.radial)
        newChild.label.add(pv.Label)
            .visible(function(d){ return d.lastChild == null })
        graphRoot.render()
    },
    force:function(){
        var graphRoot = Master.panel();
        var nodes = []
        var links = []
        var ptr = 0
        _.each(frequenciesByAuthor, function(f,a){
            nodes.push({freq:f,author:a})
            links.push({source:ptr++,target:0,value:25})
        })
        var newChild = graphRoot.add(pv.Layout.Force)
            .nodes(nodes)
            .links(links)
        newChild.link.add(pv.Line)
        newChild.node.add(pv.Dot)
            .size(function(d){return dotScale(d.freq)})
            .fillStyle("red")
            .anchor("center")
            .add(pv.Label)
            .text(function(d){return d.author})
        graphRoot.render()
    }
}
var Conversations = {
    color:pv.Scale.linear(0,10).range("white","red"),
    radial:function(node){
        var graphRoot = Master.panel()
        var author = node.nodeName;
        var data = detailsByAuthor[author]
        var nodes = pv.dom(data)
            .leaf(function(d){
                return "jid" in d
            })
            .nodes()
        var newChild = graphRoot.add(pv.Layout.Tree)
            .nodes(nodes)
            .orient("radial")
            .depth(300)
            .breadth(50)
        newChild.link.add(pv.Line)
        newChild.node.add(pv.Dot)
            .fillStyle(function(d){
                if(!d.nodeValue) return "white"
                return Conversations.color(d.nodeValue.authorCount)})
            .size(function(d){
                if(!d.nodeValue) return 0
                return d.nodeValue.contentVolume})
            .event("click",function(node){
                conversationJoined(node.nodeValue)
            })
        newChild.label.add(pv.Label)
            .text(function(d){
                if(!d.nodeValue) return ""
                return d.nodeValue.title})
        graphRoot.render()
    },
    packed:function(node){
        var graphRoot = Master.panel()
        var author = node.nodeName;
        var data = detailsByAuthor[author]
        var nodes = pv.dom(data)
            .leaf(function(d){
                return "jid" in d
            })
            .nodes()
        var newChild = graphRoot.add(pv.Layout.Pack)
            .nodes(nodes)
            .size(function(d){
                if(!d.nodeValue) return 0
                return d.nodeValue.contentVolume * 4})
            .top(-50)
            .bottom(-50)
            .order(null)
        newChild.node.add(pv.Dot)
            .fillStyle(function(d){
                if(!d.nodeValue) return "white"
                return Conversations.color(d.nodeValue.authorCount)})
            .event("click",function(node){
                conversationJoined(node.nodeValue)
            })
            .anchor("center")
            .add(pv.Label)
            .text(function(d){
                if(!d.nodeValue) return ""
                return d.nodeValue.title})
        graphRoot.render()
   }
}
var Conversation = (function(){
    Commands.add("conversationJoined",function(conversation){
        $("#visualSlideDisplay").remove()
        $('body').append($("<div id='visualSlideDisplay'></div>"))
        Conversation.slides(conversation)
    })
    return {
        slides:function(conversation){
            var jid = parseInt(conversation.jid)
            var slideCount = parseInt(conversation.slideCount)
            var nodes = pv.range(jid+1, jid+slideCount)
            var padding = 20
            var yAxis = pv.Scale.linear(1,slideCount).range(padding,height-padding)
            $("#visualSlideDisplay").height(slideCount * height).width(width)
            var graphRoot = new pv.Panel()
                .canvas("visualSlideDisplay")
                .width(width) 
                .height(slideCount * height)
            graphRoot.add(pv.Image)
                .data(nodes)
                .top(function(){return this.index * height})
                .width(width)
                .height(height)
                .url(function(i){
                    return "snapshot?width="+width+"&height="+height+"&server="+server+"&slide="+i
                })
                .strokeStyle("black")
            graphRoot.render()
        },
        analysis:function(node){
            $.getJSON("conversation?server="+server+"&jid="+parseInt(node.nodeValue.jid),Conversation.overflowedDetailOnXAxis)        
        },
        horizonSlides:function(content){},
        overflowedDetailOnXAxis:function(content){
            var data = content.details
            var times = _.pluck(data, "timestamp").map(function(t){return parseInt(t)})
            var min = _.min(times)
            var max = _.max(times)
            var timeSpan = (max - min) / 1000//Milis to seconds
            var master = Master.panel()
                .width(timeSpan)
                .height(height)
                .overflow("visible")
                .data(data)
            var timeToX = pv.Scale.linear(_.min(times),_.max(times)).range(0,timeSpan)//1 px per second
            master.add(pv.Dot)
                .size(50)
                .fillStyle("red")
                .top(250)
                .left(function(d){return timeToX(d.timestamp)})
            master.render()
        },
        masterDetailOnXAxis:function(content){
            var data = content.details
            var master = Master.panel()
                .width(width)
                .height(height)
            var i = {x:200,dx:100}
            var focusHeight = 900
            var contextHeight = 100
            var fx = pv.Scale.linear().range(0,width)
            var fy = pv.Scale.linear().range(0,focusHeight)
            var times = _.pluck(data, "timestamp").map(function(t){return parseInt(t)})
            var dateToX = pv.Scale.linear(_.min(times), _.max(times)).range(0,width)
            var slides = _.pluck(data, "slide").map(function(t){return parseInt(t)})
            var slideToY = pv.Scale.linear(_.min(slides), _.max(slides)).range(0,height)

            var focus = master.add(pv.Panel)
                .def("init",function(){
                    var d1 = dateToX.invert(i.x)
                    var d2 = dateToX.invert(i.x + i.dx)
                    var dd = data.slice(
                        Math.max(0,pv.search.index(data,d1,function(d){return d.timestamp}) - 1),
                            pv.search.index(data,d2,function(d){return d.slide}) + 1)
                    fx.domain(d1,d2)
                    fy.domain([0,pv.max(dd,function(d){return d.slide})])
                    console.log(dd.length)
                    return dd
                })
                .top(0)
                .height(focusHeight)

            focus.add(pv.Panel)
                .overflow("hidden")
                .add(pv.Dot)
                    .data(function(){
                        return focus.init()
                    })
                    .left(function(d){ 
                        return fx(d.timestamp)
                    })
                    .bottom(function(d){
                        return fy(d.slide)
                    })
                    .size(100)
                    .fillStyle("red")
                    
            var context = master.add(pv.Panel)
                .bottom(0)
                .height(contextHeight)
                .fillStyle("blue")
                        
            context.add(pv.Panel)
                .data([i])
                .cursor("crosshair")
                .events("all")
                .event("mousedown", pv.Behavior.select())
                .event("select", focus)
              .add(pv.Bar)
                .left(function(d){
                    return d.x
                })
                .width(function(d){ 
                    return d.dx
                })
                .fillStyle("rgba(255, 128, 128, .4)")
                .cursor("move")
                .event("mousedown", pv.Behavior.drag())
                .event("drag", focus);
                
            focus.add(pv.Rule)
                .data(function(){return fx.ticks()})
                .left(fx)
                .strokeStyle("black")
                .anchor("bottom")
                .add(pv.Label)
                    .text(fx.tickFormat())
            master.render()
        }
    }
})()
var ClassRoom =(function(){
    var tick = 10000
    var ONE_HOUR = 3600000
    var start = new Date(new Date().setHours(14)).setMinutes(0)
    function behaviour(group){
        if(!("contentType" in group.parameters)) group.parameters.contentType = "ink"
        if(!("likelihoodOfWriting" in group.parameters)) group.parameters.likelihoodOfWriting = 0.2
        if(!("sheepFactor" in group.parameters)) group.parameters.sheepFactor = 0.4
        if(!("conchHandoffProbability" in group.parameters)) group.parameters.conchHandoffProbability = 0.1
        if(!("beginAfterSeconds" in group.parameters)) group.parameters.beginAfterSeconds = 500
        if(!("contentType" in group.parameters)) group.parameters.contentType = "ink"
        var beginning = new Date(start).getTime()
        var end =  (beginning + ONE_HOUR) - group.parameters.endAfterSeconds * 1000;
        var groupMessages = []
        var conchHolder = group.members[0]
        for(var time = beginning + group.parameters.beginAfterSeconds * 1000; time < end; time += tick){
            if(Math.random() < group.parameters.likelihoodOfWriting)
                groupMessages.push({
                    contentType:group.parameters.contentType,
                    author : conchHolder,
                    timestamp : time,
                    slide : (groups.indexOf(group)).toString(),
                    standing:group.members.indexOf(conchHolder)
                })
            if(Math.random() < group.parameters.conchHandoffProbability)
                conchHolder = group.members[Math.floor(Math.random() * (group.members.length -1))]
        }
        return groupMessages
    }
    var groups = [
        {
            members:["Albert"],
            parameters:{
                likelihoodOfWriting:0.5,
                conchHandoffProbability : 0,
                beginAfterSeconds:0,
                endAfterSeconds:3090
            }
        },
        {
            members:["Tom","Dick","Harry","Ramesh"],
            parameters:{
                endAfterSeconds:510
            }
        },
        {
            members:["Stanley"],
            parameters:{
                likelihoodOfWriting:0.1,
                conchHandoffProbability:0,
                endAfterSeconds:1230
            }
        },
        {
            members:["Dora","Diego","Denver"],
            parameters:{
                endAfterSeconds:200
            }
        },
        {
            members:["Ptolemy","Frencisco","Aoki"],
            parameters:{
                endAfterSeconds:0,
                contentType:"text"
            }
        }
    ]
    return {
        groups:groups,
        messages:function(){
            return _.flatten(_.map(groups,
                function(group){
                    return behaviour(group)
                }))
             }
        }
})()
_.flatMap = function(list,func){
    return _.flatten(_.map(list,func))
}
var ProofOfConcept = (function(){
    var colorByStudent = pv.Colors.category19().by(function(d){return d.author})
    var data = ClassRoom.messages()
    var week = 6
    var xAxis = pv.Scale.linear(0,week).range(50,width-50)
    var yAxis = pv.Scale.linear(0,100).range(50,height-50)
    function standings(user, standing){
        return _.reduce(pv.range(1,week), function(acc,i){
            var previousStandings = acc[0] 
            var previousStanding = acc[1] 
            var newStandingValue = Math.max(0,previousStanding - (Math.random() * 0.5))
            var newStanding = {author:user,x:week-i,y:newStandingValue}
            previousStandings.push(newStanding)
            return [previousStandings, newStandingValue]
        }, [[{author:user,x:0,y:0}],standing])[0].sort(function(a,b){
            return a.x > b.x
        })
    }
    return {
        usersByStandingOverTime:function(){
            var users  = _.flatMap(ClassRoom.groups, function(g){
                                return _.map(g.members, function(user,standing){
                                    return {author:user,standing:standings(user, standing)}
                                })
                            })
            var root = Master.panel()
                .width(width)
                .height(height)
                .data(users)
            root.add(pv.Rule)
                .data(xAxis.ticks())
                .bottom(0) 
                .left(xAxis)
                .height(20)
                .strokeStyle("black")
                .anchor("top")
                .add(pv.Label)
                    .text(function(d){return "Wk "+(this.index+1)})
            root.add(pv.Rule)
                .data(yAxis.ticks())
                .bottom(yAxis)
                .width(20)
                .strokeStyle("black")
                .anchor("right")
                .add(pv.Label)
            root.add(pv.Panel)
                .add(pv.Line)
                    .strokeStyle(colorByStudent)
                    .data(function(d){
                        return d.standing})
                    .lineWidth(3)
                    .left(function(d){return xAxis(d.x)})
                    .bottom(function(d){return yAxis(d.y * 20)})
            root.render()
        },
        contentBySlideOverTime:function(){
            var times = _.map(_.pluck(data, "timestamp"),function(s){return parseInt(s)})
            var slides = _.uniq(_.map(_.pluck(data, "slide"),function(s){return parseInt(s)}))
            var xAxis = pv.Scale.linear(_.min(times),_.max(times)).range(50,width - 50)
            var yAxis = pv.Scale.linear(0,_.max(slides)).range(50,height - 50)
            var slideHeight = Math.round((height - 100) / slides.length)
            var lineTicks = slides.length -1
            var lineHeight = Math.round((height - 100) / lineTicks)
            var subYAxis = pv.Scale.linear(0,5).range(5,lineHeight - 5)
            var lightLines = pv.Scale.linear(0,lineTicks).range(0,lineTicks)
            var root = Master.panel()
                .width(width)
                .height(height)
                .data(data)
            root.add(pv.Rule)
                .data(xAxis.ticks())
                .bottom(0) 
                .left(xAxis)
                .height(20)
                .strokeStyle("black")
                .anchor("top")
                .add(pv.Label)
                    .text(function(d){
                        return pv.Format.date("%r")(new Date(d))})
            root.add(pv.Rule)
                .data(lightLines.ticks(lineTicks))
                .width(width)
                .left(0)
                .strokeStyle("lightgray")
                .top(function(){
                    return 50 + (this.index * lineHeight) - lineHeight / 2
                })
            root.add(pv.Rule)
                .data(yAxis.ticks(_.max(slides)))
                .bottom(yAxis)
                .width(20)
                .strokeStyle("black")
                .anchor("right")
                .add(pv.Label)
                    .text(yAxis.tickFormat)
            root.add(pv.Dot)
                .size(50)
                .shape(function(d){
                    switch(d.contentType){
                        case "ink":return "cross"
                        case "image":return "circle"
                        case "text":return "diamond"
                    }
                })
                .top(function(d){
                    return height - (yAxis(parseInt(d.slide)) + subYAxis(d.standing))
                })
                .left(function(d){
                    return xAxis(parseInt(d.timestamp))
                })
                .fillStyle(colorByStudent)
                .strokeStyle(colorByStudent)
                .event("click",function(d){
                    alert( 
                        new Date(parseInt(d.timestamp)).toString() + 
                        " @ " +
                        d.slide +
                        " by "+
                        d.author)
                })
            root.render()
       }
    }
})() 
$(function(){
    //ProofOfConcept.usersByStandingOverTime()
    Authors.radial()
})
