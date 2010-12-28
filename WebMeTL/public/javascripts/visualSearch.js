Marketplace.add({
    label:'Visual navigation',
    icon:'visualNav.png',
    add:function(){
        var svg = pv.SvgScene.expect(null,'svg',{id:'authorsSvgHost',viewBox:sprintf('0 0 %d %d',850,800)})
        $('#main').append(svg)
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
        var Conversations = {
            color:pv.Scale.linear(0,10).range("white","red"),
            radial:function(node){
                var display = function(data){
                    var id = "visualSearchConversations"
                    $('#'+id).remove()
                    $('#main').append($(sprintf("<div id='%s'></div>",id)))
                    var graphRoot = 
                        new pv.Panel()
                            .canvas(id)
                            .left(0)
                            .top(0)
                            .bottom(0)
                            .right(0)
                            .width(width)
                            .height(height)
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
                            Commands.fire('conversationJoined',node.nodeValue)
                        })
                    newChild.label.add(pv.Label)
                        .text(function(d){
                            if(!d.nodeValue) return ""
                            return d.nodeValue.title})
                    graphRoot.render()
                }
                var author = node.nodeName;
                var data = detailsByAuthor[author]
                Commands.add("filterDisplayedConversations",function(searchTerm){
                    display(_.filter(data,function(conversation){
                        return conversation.title.indexOf(searchTerm) >= 0    
                    }))
                })
                display(data) 
            }
        }
        var Authors = {
            colors:_.reduce(_.keys(detailedAuthors.conversationSummaries), function(acc,item,index){
                var availableColors = pv.Colors.category19().range();
                acc[item] = availableColors[index % availableColors.length]
                return acc;
            },{}),
            radial:(function(){
                var color = function(d){
                    if(!d.nodeName) return "black"
                    return Authors.colors[d.nodeName]
                }
                var _data = clusteredNodes
                var graphRoot = 
                    new pv.Panel()
                        .canvas('authorsSvgHost')
                        .left(0)
                        .top(0)
                        .bottom(0)
                        .right(0)
                        .width(900)
                        .height(800)
                var tree = graphRoot.add(pv.Layout.Tree)
                    .nodes(function(){
                        return _data
                    })
                    .orient("radial")
                tree.link.add(pv.Line)
                tree.node.add(pv.Dot)
                    .fillStyle(color)
                    .size(function(d){
                        return dotScale(d.nodeValue)})
                    .event("click",Conversations.radial)
                tree.label.add(pv.Label)
                    .visible(function(d){ return d.lastChild == null })
                var display = function(data){
                    _data = pv.dom(data).nodes()
                    tree.reset()
                    graphRoot.render()
                }
                Commands.add("filterDisplayedAuthors",function(searchTerm){
                    if(searchTerm.length == 0){
                        display(clusteredFrequencies)
                    }
                    else{
                        var raw = frequenciesByAuthor
                        display(_.reduce(_.filter(_.keys(raw),function(author){
                            return author.indexOf(searchTerm) >= 0
                        }),function(acc,author){
                            acc[author] = raw[author]        
                            return acc
                        },{}))
                    }
                })
                return function(){
                    var raw = detailedAuthors.conversationSummaries
                    var expanded = _.reduce(_.keys(raw),function(acc,author){
                        var authorDetails = raw[author]
                        var band = Math.floor(parseFloat(authorDetails.conversationCount) / 6);
                        acc[author] = deepen(band,author,_.reduce(authorDetails.conversations.listing,function(acc,conversation){
                            acc[conversation.title] = conversation.contentVolume
                            return acc
                        },{}))
                        return acc
                    },{})
                    display(clusteredFrequencies)
                }
            })()
        }
        Commands.add("searchForConversations",function(){
            Authors.radial()
        })
        Commands.fire('searchForConversations')
    }
})
