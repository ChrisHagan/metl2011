var width = window.innerWidth
var height = window.innerHeight
_.flatMap = function(list,func){
    return _.flatten(_.map(list,func))
}
