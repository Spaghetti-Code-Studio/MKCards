interface ICard : IOrderable<ICard>
{
    /// <summary>
    /// The SVG image displayed at the front of the card
    /// </summary>
    string FrontFace { get; }

    /// <summary>
    /// The SVG image displayed at the back of the card
    /// </summary>
    string BackFace { get; }

    /// <summary>
    /// Reports global visibility:
    /// visible cards show their front fact, invisible cards their back face
    /// </summary>
    bool Visible { get; }

    // TODO: some players can see a card while others cannot, there should be a method that takes a players
    // and considers the visibility for them specifically
    // bool IsVisible(IPlayer);
}