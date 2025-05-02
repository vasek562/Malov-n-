Instrukce na použití : Používej myš a horní ovládací prvky k výběru nástrojů a kreslení.
Funkce
    Kreslení čáry, obdélníku a elipsy

    Nástroj „Guma“ (Eraser)

    Nástroj „Výplň“ (Fill) na tu jsem použil algoritmus flood fill

Možnost volby:

    Barvy

    Stylu čáry (plná, čárkovaná, tečkovaná)

    Tloušťky čáry

Ovládání
    Levé tlačítko myši: klikni a táhni pro kreslení tvarů

    Výběr nástroje: horní rozbalovací seznam (ComboBox)

    Změna barvy: kliknutím na tlačítko „Barva“

    Styl čáry: rozbalovací seznam „Styl“

    Tloušťka čáry: číselník vedle stylu čáry

Struktura kódu
  Třída MainForm
  Hlavní okno aplikace

      Obsahuje obsluhu událostí myši (MouseDown, MouseMove, MouseUp)

      Obsahuje kreslení pomocí Paint události
  
      Obsahuje metodu InitializeUI() pro vytvoření ovládacích prvků

Abstraktní třída Shape
      Slouží jako základ pro kreslitelné objekty

    Uchovává počáteční a koncový bod, barvu, tloušťku a styl čáry

    Obsahuje abstraktní metodu Draw(Graphics g)

Potomci třídy Shape
            LineShape – kreslí čáru

            RectangleShape – kreslí obdélník

            EllipseShape – kreslí elipsu
