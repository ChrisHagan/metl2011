var MeTL_Ink = function(context){
    var id = "metl_ink_generated_canvas"
    $('.'+id).remove()
    /*Canvas seems to need to be initialized with dimensions already in place or it behaves oddly with scaling*/
    var canvas = $("<canvas class='"+id+"' width='"+context.width()+"px' height='"+context.height()+"px'></canvas>").css(context.offset()).css("position","absolute").css("z-index",9)
    context.after(canvas)
    var pen = canvas[0].getContext( "2d" );
    pen.strokeStyle = "red"
    pen.strokeWidth = 4
    var lastPenPoint = null;
    var isIPhone = (new RegExp( "iPhone|iPad", "i" )).test(navigator.userAgent);
    var getCanvasLocalCoordinates = function( pageX, pageY ){
        var position = canvas.offset();
        return({
            x: (pageX - position.left),
            y: (pageY - position.top)
        });
    };
    var getTouchEvent = function( event ){
        return( isIPhone ?  window.event.targetTouches[ 0 ] : event);
    };
    var onTouchStart = function( event ){
        var touch = getTouchEvent( event );
        var localPosition = getCanvasLocalCoordinates( touch.pageX, touch.pageY);
        lastPenPoint = {
            x: localPosition.x,
            y: localPosition.y
        };
        pen.beginPath();
        pen.moveTo( lastPenPoint.x, lastPenPoint.y );
        canvas.bind( (isIPhone ? "touchmove" : "mousemove"), onTouchMove);
        canvas.bind( (isIPhone ? "touchend" : "mouseup"), onTouchEnd);
    };
    var onTouchMove = function( event ){
        var touch = getTouchEvent( event );
        var localPosition = getCanvasLocalCoordinates( touch.pageX, touch.pageY);
        lastPenPoint = {
            x: localPosition.x,
            y: localPosition.y
        };
        pen.lineTo( lastPenPoint.x, lastPenPoint.y );
        pen.stroke();
    };
    var onTouchEnd = function( event ){
        canvas.unbind( (isIPhone ? "touchmove" : "mousemove"));
    canvas.unbind( (isIPhone ? "touchend" : "mouseup"));
};
canvas.bind( (isIPhone ? "touchstart" : "mousedown"), function( event ){
    onTouchStart( event );
    return( false );
});
}
Commands.add("slideRendered",function(details){
    MeTL_Ink($(details.img))
})

