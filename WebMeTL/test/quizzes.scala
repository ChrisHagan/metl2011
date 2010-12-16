import org.junit._
import org.junit.Assert._
import play.test._
import MeTL._
import net.liftweb.json.JsonDSL._
import net.liftweb.json.JsonAST._
import net.liftweb.json.JsonParser._
class QuizSerialization extends UnitTest with JsonTest{
    val quiz1 = Quiz("First",100000,1,"guy1",List(QuizOption("Choose this","Only choice",true,"0 0 0 255")))
    @Test
    def toJsonSimple={
        val expected = """{"title":"First","timestamp":100000,"conversation":1,"author":"guy1","options":[{"name":"Choose this","text":"Only choice","correct":true,"color":"0 0 0 255"}]}"""
        val actual = pretty(render(quiz1.toJson))
        jsonEquals(expected, actual)
    }
}

