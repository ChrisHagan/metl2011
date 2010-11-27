colors = _.reduce(authors, function(acc,index,item){
    var availableColors = pv.Colors.category19().range();
    acc[item] = availableColors[index % availableColors.length]
    return acc;
},{});
conversations = {"chagan":[{x:120,y:145,volume:80}]};
function active(d, v) {
    drillDown.data = detailedAuthors[d.nodeName].conversation;
    drillDown.render();
}
var nodes = pv.dom(authors).root("authors").nodes();
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
