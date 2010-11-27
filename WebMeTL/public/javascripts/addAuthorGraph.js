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
var clusteredNodes = pv.dom(cluster(frequenciesByAuthor,5)).nodes();
var dotScale = pv.Scale.linear(_.min(_.values(frequenciesByAuthor)), _.max(_.values(frequenciesByAuthor))).range(20,250)

function active(d, v) {
    drillDown.data = detailedAuthors[d.nodeName].conversation;
    drillDown.render();
}
function color(d){
    return colors[d.nodeName];
}
function cluster(subject, maxClusters){
    var subjectValues = _.values(subject)
    var divisor = Math.round(_.max(subjectValues) / maxClusters);
    return _.reduce(subject, function(acc, v, k){
        var group = Math.floor(v / divisor)
        if(!acc[group]) acc[group] = {}
        acc[group][k] = v
        return acc
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
        .depth(130)
        .orient("radial")
    nodeTree.node.add(pv.Dot)
        .fillStyle(color)
        .size(function(d){return dotScale(d.nodeValue)})
    nodeTree.label.add(pv.Label)
    nodeTree.link.add(pv.Line)
    parent.render()
}

nodeLink(new pv.Panel().canvas("fig"), clusteredNodes)
