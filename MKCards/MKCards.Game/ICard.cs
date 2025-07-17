// TODO: we need a dedicated way to manage this, just a simple string wrapper that handles the viewBox should suffice 
using SVGCardImage = System.String;

enum CardAttributeType
{
    Bool,       // true or false
    Integer,    // basic signed numbers
    String,     // arbitrary string
}

interface ICard : IOrderable<ICard>
{
    /// <summary>
    /// The SVG image displayed at the front of the card
    /// </summary>
    SVGCardImage FrontFace { get; }

    /// <summary>
    /// The SVG image displayed at the back of the card
    /// </summary>
    SVGCardImage BackFace { get; }

    /// <summary>
    /// Card attribute mapping attribute names to typed values
    /// </summary>
    IDictionary<string, (CardAttributeType, object)> CardAttributes; // TODO: inconsistent state?
}