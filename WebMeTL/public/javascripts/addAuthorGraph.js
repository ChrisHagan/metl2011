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
var clusteredFrequencies = cluster(frequenciesByAuthor,6);
var clusteredNodes = pv.dom(clusteredFrequencies).nodes();
var dotScale = pv.Scale.linear(_.min(_.values(frequenciesByAuthor)), _.max(_.values(frequenciesByAuthor))).range(20,1000)
var marginScale = pv.Scale.linear().range(-100,100)

function deepen(level,k,v){
    var obj = {}
    obj[k] = v
    return _.reduce(_.range(level),function(acc,i){
        var wrapper = {}
        wrapper[k] = acc
        return wrapper 
    },obj) 
}
function cluster(subject, maxClusters){
    var subjectValues = _.values(subject)
    var limit = _.max(subjectValues)
    var divisor = Math.round(limit / maxClusters);
    return _.reduce(subject, function(acc, v, k){
        var group = Math.min(Math.floor(v / divisor) + 2, maxClusters)
        var deepGroup = deepen(group,k,v)
        return _.extend(acc, deepGroup)
    }, {});
}

var Master = {
    panel:function(){
        return new pv.Panel()
            .canvas("fig")
            .left(0)
            .top(0)
            .bottom(0)
            .right(0)
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
        .nodes(this.data);
        treemap.leaf.add(pv.Panel)
            .fillStyle(Authors.color)
            .strokeStyle("#fff")
            .lineWidth(1)
            .event("click",detail)
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
            .breadth(20)
        newChild.link.add(pv.Line)
        newChild.node.add(pv.Dot)
            .fillStyle(function(d){
                if(!d.nodeValue) return "white"
                return Conversations.color(d.nodeValue.authorCount)})
            .size(function(d){
                if(!d.nodeValue) return 0
                return d.nodeValue.contentVolume})
            .event("click",Conversation.slides)
        newChild.label.add(pv.Label)
            .text(function(d){
                if(!d.nodeValue) return ""
                return d.nodeValue.title})
        graphRoot.render()
        Breadcrumb.add(author, function(){Conversations.radial(node)})
    }
}
var Conversation = {
    slides:function(node){
        var side = 1024
        var conversation = node.nodeValue
        var jid = parseInt(conversation.jid)
        var slideCount = parseInt(conversation.slideCount)
        var nodes = pv.range(jid+1, jid+slideCount)
        var graphRoot = new pv.Panel()
            .canvas("slideDisplay")
            .width(side) 
            .height(slideCount * side)
        graphRoot.add(pv.Image)
            .data(nodes)
            .top(function(){return this.index * side})
            .width(side)
            .height(side)
            .url(function(i){
                return "http://localhost:8080?width="+side+"&height="+side+"&server=deified&slide="+i
            })
            .strokeStyle("black")
        graphRoot.render()
        Breadcrumb.add(conversation.title, function(){Conversation.slides(node)})
    }
}
var Breadcrumb = (function(){ 
    var trail = []
    var container = $("#breadcrumb")          
    return {
        add: function(label, func){
            if(_.contains(_.pluck(trail, "label"), label)) return;
            trail.push({label:label, func:func})
            this.render()
        },
        render:function(){
            container.html("")
            var recentCrumbs = [{label:"All authors", func:Authors.radial}]
            _.each(trail.slice(-5), function(crumb){recentCrumbs.push(crumb)})
            _.each(recentCrumbs, function(crumb){
                container.append($("<span />").append(crumb.label.slice(0,15) + "->").click(function(){
                    scroll(0,0)
                    crumb.func()
                }))
            })
        }
    }
})()
Authors.radial()
Breadcrumb.render()
