(function(){
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
    _.flatMap = function(list,func){
        return _.flatten(_.map(list,func))
    }
    Commands.add("searchForConversations",function(){
        Authors.radial()
    })
})()
