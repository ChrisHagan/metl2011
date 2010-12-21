var BubbleTree = (function(){
    var id='bubbleHost'
    var actOnBubble = function(){
        var options = $("<div class='bubbleOptions'></div>")
        _.each(["upArrow.png","downArrow.png"],function(src){
            options.append($("<img src='/public/images/"+src+"'/>").css('height','50px'))
        })
        $(this).append(options)
    }
    var bubbleHost = $("<div id='"+id+"'></div>").bubble().css({position:'absolute'}).draggable({revert:'valid'}).droppable({accept:'.canDropOnBubbleTree'})
    $('body').append(bubbleHost)
    bubbleHost.hide()
    var px = function(s){return s + 'px'}
    Commands.add("bubble",function(bubble){
        bubbleHost.show()
        var b = $("<div></div>").text(bubble.text)
        b.click(actOnBubble)
        bubbleHost.append(b.bubble().addClass('canDropOnSlides').addClass('canDropOnBubbleTree'))
    })
})()
