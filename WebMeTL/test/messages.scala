import org.junit._
import org.junit.Assert._
import play.test._
import MeTL._
import MeTL.MessageReductor._
import net.liftweb.json.JsonDSL._
import net.liftweb.json.JsonAST._
import net.liftweb.json.JsonParser._

class MessageClumping extends UnitTest{
    val message1 = Message("ink", 100000, 1, "guy1",1)
    val message2 = Message("ink", 100050, 2, "guy2",2)
    val message3 = Message("ink", 100100, 3, "guy3",3)
    val message4 = Message("ink", 100150, 4, "guy4",4)
    val message5 = Message("ink", 200150, 5, "guy5",4)
    val message6 = Message("ink", 200150, 6, "guy6",4)
    val message7 = Message("ink", 200250, 7, "guy7",4)
    val message8 = Message("ink", 200350, 8, "guy8",5)
    val message9 = Message("ink", 200450, 9, "guy9",5)
    val message10 = Message("ink", 300450, 9, "with9",5)
    val message11 = Message("ink", 500450, 9, "with9_2",5)
    val message12 = Message("ink", 100450, 9, "with9_3",5)
    @Test
    def simpleClumping={
        assertEquals(SimpleClump(List()).duration, JInt(0))
        assertEquals(SimpleClump(List()), clump(List()))
        assertEquals(SimpleClump(List(message1)), clump(List(message1)))
        assertEquals(SimpleClump(List(message1,message2)), clump(List(message1,message2)))
        assertEquals(SimpleClump(List(message4, message5)), clump(List(message1,message2,message3,message4,message5)))
        assertEquals(SimpleClump(List(message4, message9)), clump(List(message1,message2,message3,message4,message5,message6,message7,message8,message9)))
    }
    @Test 
    def complexClumping={
        assertEquals(DetailedClump(List(List())).duration, JInt(0))
        assertEquals(
            DetailedClump(List(List())), 
            clump(List(), true))
        assertEquals(
            DetailedClump(List(List(message1))), 
            clump(List(message1), true))
        assertEquals(
            DetailedClump(List(List(message1,message2))), 
            clump(List(message1,message2), true))
        assertEquals(
            DetailedClump(List(List(message1,message2,message3,message4), List(message5))), 
            clump(List(message1,message2,message3,message4,message5), true))
        assertEquals(
            DetailedClump(List(
                List(message1,message2,message3,message4), 
                List(message5,message6,message7,message8,message9))), 
            clump(List(message1,message2,message3,message4,message5,message6,message7,message8,message9), true))
    }
    @Test
    def toJsonSimple={
        val expected = """[{
            "contentType":"ink", 
            "standing":1,
            "timestamp":100000, 
            "slide":1, 
            "author":"guy1"
        }]"""
        val actual = pretty(render(SimpleClump(List(message1)).toJson))
        jsonEquals(expected, actual)
    }
    @Test
    def toJsonSummary={
        val expected = """
        [
            {
                "timestamp":100000,
                "duration":50,
                "authors":["guy1","guy2"],
                "ink":2,
                "image":0,
                "text":0,
                "slides":[1,2]
            },
            {
                "timestamp":200150,
                "duration":0,
                "authors":["guy5","guy6"],
                "ink":2,
                "image":0,
                "text":0,
                "slides":[5,6]
            }
        ]
        """
        val actual = pretty(render(DetailedClump(List(List(message2,message1),List(message5,message6))).toJsonSummary))
        jsonEquals(expected,actual)
    }
    @Test
    def toJsonFull={
        val expected = """
        {
            "duration":100150,
            "summary":
            [
                {
                    "timestamp":100000,
                    "duration" : 50,
                    "authors":["guy1","guy2"],
                    "ink":2,
                    "image":0,
                    "text":0,
                    "slides":[1,2]
                },
                {
                    "timestamp":200150,
                    "duration" : 0,
                    "authors":["guy5","guy6"],
                    "ink":2,
                    "image":0,
                    "text":0,
                    "slides":[5,6]
                }
            ],
            "details":
            [
                [
                    {
                        "contentType":"ink", 
                        "standing":1,
                        "timestamp":100000, 
                        "slide":1, 
                        "author":"guy1"
                    },
                    {
                        "contentType":"ink", 
                        "standing":2,
                        "timestamp":100050, 
                        "slide":2, 
                        "author":"guy2"
                    }
                ],
                [
                    {
                        "contentType":"ink", 
                        "standing":4,
                        "timestamp":200150, 
                        "slide":5, 
                        "author":"guy5"
                    },
                    {
                        "contentType":"ink", 
                        "standing":4,
                        "timestamp":200150, 
                        "slide":6, 
                        "author":"guy6"
                    }
                ]
            ]
        }
        """
        val actual = pretty(render(DetailedClump(List(List(message1,message2),List(message5,message6))).toJsonFull))
        jsonEquals(expected,actual)
    }
    @Test
    def toJsonComplex={
        val expected = """
        [
            [
                {
                    "contentType":"ink", 
                    "standing":1,
                    "timestamp":100000, 
                    "slide":1, 
                    "author":"guy1"
                },
                {
                    "contentType":"ink", 
                    "standing":2,
                    "timestamp":100050, 
                    "slide":2, 
                    "author":"guy2"
                }
            ],
            [
                {
                    "contentType":"ink", 
                    "standing":4,
                    "timestamp":200150, 
                    "slide":5, 
                    "author":"guy5"
                },
                {
                    "contentType":"ink", 
                    "standing":4,
                    "timestamp":200150, 
                    "slide":6, 
                    "author":"guy6"
                }
            ]
        ]
        """
        val actual = pretty(render(DetailedClump(List(List(message1,message2),List(message5,message6))).toJson))
        jsonEquals(expected,actual)
    }
    @Test
    def bySlideSimple={
        val actual = MessageReductor.clumpSlides(List(message1,message2,message3,message9,message10,message11,message12))
        val expected = DetailedClump(Seq(Seq(message1),Seq(message2),Seq(message3),Seq(message9,message10,message11,message12)))
        assertEquals(actual,expected)
    }
    private def jsonEquals(expected:String,actual:String)={
        val errorText = "Expected "+expected+" but was "+actual
        assertEquals(errorText,parse(expected),parse(actual))
    }
}
