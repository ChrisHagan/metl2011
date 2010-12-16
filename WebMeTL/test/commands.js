$(function(){
    test("Commands",function(){
        var dummy = {}
        expect(3)
        var Commands = CommandInserter(dummy)
        Commands.add("anEvent",function(){
            ok(true)
        })
        Commands.add("anEvent",function(){
            ok(true)
            return true
            //This handler finishes the chain
        })
        //We should not evaluate this handler
        Commands.add("anEvent",function(){
            ok(false)
        })
        ok("anEvent" in dummy,"Events register to the context")
        dummy.anEvent()
    })
})
