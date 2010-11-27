colors = _.reduce(_.keys(detailedAuthors.conversationSummaries), function(acc,item,index){
    var availableColors = pv.Colors.category19().range();
    acc[item] = availableColors[index % availableColors.length]
    return acc;
},{});
function active(d, v) {
    drillDown.data = detailedAuthors[d.nodeName].conversation;
    drillDown.render();
}
var frequenciesByAuthor = _.reduce(detailedAuthors.conversationSummaries, 
    function(acc, v, k){
        acc[k] = v.conversationCount;
        return acc;
    }, {});
var nodes = pv.dom(frequenciesByAuthor).nodes();
var color = function(d){
    return colors[d.nodeName];
};
var authors = new pv.Panel().canvas("fig");
var treemap = authors.add(pv.Layout.Treemap)
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
authors.render();

var drillDown = new pv.Panel().canvas("overlay")
    .add(pv.Dot)
    .left(function(d){
            return d.x})
    .top(function(d){
            return d.y})
    .size(function(d){
            return d.z})
    .fillStyle("#fff");
