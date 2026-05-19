namespace lab;

public enum SegmentType
{
    // Для максимального прискорення
    Straight, 
    // Для уповільнення та проходження повороту
    Corner,   
    // Для фіксованої мінімальної швидкості
    PitLane,  
    // Стартова/Фінішна пряма
    StartFinish
}