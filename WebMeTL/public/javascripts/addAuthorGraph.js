var colors = _.reduce(_.keys(detailedAuthors.conversationSummaries), function(acc,item,index){
    var availableColors = pv.Colors.category19().range();
    acc[item] = availableColors[index % availableColors.length]
    return acc;
},{});
var frequenciesByAuthor = _.reduce(detailedAuthors.conversationSummaries, 
    function(acc, v, k){
        acc[k] = v.conversationCount;
        return acc;
    }, {});
var flatNodes = pv.dom(frequenciesByAuthor).nodes();
var clusteredFrequencies = cluster(frequenciesByAuthor,10);
var clusteredNodes = pv.dom(clusteredFrequencies).nodes();
var dotScale = pv.Scale.linear(_.min(_.values(frequenciesByAuthor)), _.max(_.values(frequenciesByAuthor))).range(20,1000)

function active(d, v) {
    drillDown.data = detailedAuthors[d.nodeName].conversation;
    drillDown.render();
}
function color(d){
    return colors[d.nodeName];
}
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
        var group = Math.floor(v / divisor)
        var deepGroup = deepen(group, k,v)
        return _.extend(acc, deepGroup)
    }, {});
}
function treeMap(parent, nodes){
    var treemap = parent.add(pv.Layout.Treemap)
    .def("active", function(){ return -1})
    .nodes(nodes);
    treemap.leaf.add(pv.Panel)
        .fillStyle(color)
        .strokeStyle("#fff")
        .lineWidth(1)
        .event("click",function(d){
            active(d, !d.active);
            return this;
        })
    treemap.label.add(pv.Label);
    parent.render();
}
function nodeTree(parent, nodes){
    var nodeTree = parent.add(pv.Layout.Partition.Fill)
        .nodes(nodes)
        .size(function(d){
            return dotScale(d.nodeValue)})
        .order("descending")
        .orient("radial");
    nodeTree.node.add(pv.Wedge)
        .fillStyle(color)
        .strokeStyle("#fff")
        .lineWidth(1)
    nodeTree.label.add(pv.Label);
    parent.render();
}
function nodeLink(parent,nodes){
    var nodeTree = parent.add(pv.Layout.Tree)
        .nodes(nodes)
        .orient("radial")
    nodeTree.node.add(pv.Dot)
        .fillStyle(color)
        .size(function(d){return dotScale(d.nodeValue)})
    nodeTree.label.add(pv.Label)
    nodeTree.link.add(pv.Line)
    parent.render()
}

nodeLink(new pv.Panel().canvas("fig"), clusteredNodes)
