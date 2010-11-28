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
var detailsByAuthor = _.reduce(detailedAuthors.conversationSummaries,
    function(acc,v,k){
        acc[k] = _.reduce(v.conversations.listing, function(acc, v, k){
            acc[v.title] = v.slideCount
            return acc;
        }, {})
        return acc;
    }, {});
var flatNodes = pv.dom(frequenciesByAuthor).nodes();
var clusteredFrequencies = cluster(frequenciesByAuthor,6);
var clusteredNodes = pv.dom(clusteredFrequencies).nodes();
var dotScale = pv.Scale.linear(_.min(_.values(frequenciesByAuthor)), _.max(_.values(frequenciesByAuthor))).range(20,1000)
var marginScale = pv.Scale.linear().range(-100,100)

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
        var group = Math.min(Math.floor(v / divisor) + 2, maxClusters)
        var deepGroup = deepen(group, k,v)
        return _.extend(acc, deepGroup)
    }, {});
}
function treeMap(graphRoot, nodes){
    var treemap = graphRoot.add(pv.Layout.Treemap)
    .def("active", function(){ return -1})
    .nodes(nodes);
    treemap.leaf.add(pv.Panel)
        .fillStyle(color)
        .strokeStyle("#fff")
        .lineWidth(1)
        .event("click",detail)
    treemap.label.add(pv.Label);
    graphRoot.render();
}
function nodeTree(graphRoot, nodes){
    var newChild = graphRoot.add(pv.Layout.Partition.Fill)
        .nodes(nodes)
        .size(function(d){
            return dotScale(d.nodeValue)})
        .order("descending")
        .orient("radial");
    newChild.node.add(pv.Wedge)
        .fillStyle(color)
        .strokeStyle("#fff")
        .lineWidth(1)
    newChild.label.add(pv.Label);
    graphRoot.render();
}
function nodeLink(graphRoot,nodes){
    var newChild = graphRoot.add(pv.Layout.Tree)
        .nodes(nodes)
        .orient("radial")
    newChild.link.add(pv.Line)
    newChild.node.add(pv.Dot)
        .fillStyle(color)
        .size(function(d){
            return dotScale(d.nodeValue)})
        .event("click",detail)
    newChild.label.add(pv.Label)
        .visible(function(d){ return d.lastChild == null })
    graphRoot.render()
}

function detail(node){
    var author = node.nodeName;
    var nodes = pv.dom(cluster(detailsByAuthor[author], 10)).nodes()
    nodeLink(new pv.Panel().canvas("overlay").left(node.x).top(node.y), nodes)
}

var master = new pv.Panel().canvas("fig")
nodeLink(master, clusteredNodes)
