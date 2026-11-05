# Maison de départ — CorseVR

Scène modifiée et enregistrée : `Assets/Scenes/Prototype_Maquis.unity`.

Le joueur commence désormais à l'intérieur d'une petite maison en pierre, porte fermée, face à la sortie du maquis. L'intérieur comprend un dallage, une charpente, des fenêtres profondes, une cheminée, une table, un banc et une étagère. La maison et sa porte ont été modélisées dans Blender puis importées en FBX. Le fichier modifiable `Maison_Corse_Accueil.blend` est conservé dans ce dossier.

Sur ordinateur : Play, clic dans Game, ZQSD/flèches pour marcher, souris pour regarder. S'approcher de la porte et regarder sa poignée, puis appuyer sur **E** pour l'ouvrir ou la fermer. Un indicateur apparaît à proximité. Échap libère la souris. La porte s'ouvre vers l'extérieur et possède une collision.

Les deux anciennes maisons autour des passages sont désactivées dans leur groupe d'archive. Les deux arches et leurs transitions maquis/littoral restent actives. Le littoral n'a pas été reconstruit par cette intervention.

Le terrain a été aplani sous la maison et raccordé au début du sentier. La végétation qui pénétrait dans la pièce a été retirée de cette emprise. Le déplacement autorise l'exploration de l'intérieur puis se raccorde au corridor du sentier ; murs et meubles possèdent des collisions.

## Vérifications dans Unity

- Départ à l'intérieur : position locale `(0, 0.08, -4.45)`, caméra à 1,65 m au-dessus du joueur.
- Interaction disponible à l'approche de la porte : test positif à environ 1,75 m du seuil.
- Porte fermée : le contrôleur est bloqué à 0,39 m avant le seuil.
- Ouverture de 105° : test positif ; le contrôleur franchit le seuil et atteint le sentier à 3,61 m devant la façade.
- Parcours complet du maquis et transition vers le littoral : réussi.
- Parcours du littoral et transition retour : réussi.
- Console après les tests : aucune erreur, aucun avertissement.
- Scène enregistrée puis rechargée pour vérifier la persistance : départ intérieur, porte fermée, deux arches actives, anciennes maisons inactives. État final hors mode Play, sans modification non enregistrée.

Les captures dans `Apres/` sont des rendus Unity en 1920 × 1080, et non des images générées. L'image conceptuelle de la conversation reste une direction artistique : cette première version jouable ne reproduit pas son niveau de réalisme. L'interaction au casque et les performances sur Quest 3 restent à vérifier.

## Fichiers de reprise

- Maison : `Assets/Prototype_Maquis/Art/MaisonAccueil/`.
- Générateur : `Assets/Prototype_Maquis/Art/Editor/MaisonAccueilBuilder.cs`, méthode `Build()` en mode édition.
- Porte : `Assets/Prototype_Maquis/Scripts/AccueilDoor.cs`.
- Zone de marche intérieure : champs ajoutés à `SentierCorridor.cs`.
- Copie de la scène avant modification : `Prototype_Maquis_avant.unity` dans ce dossier.
- Copie du script de corridor antérieur : `work/maison_depart/SentierCorridor_avant.cs` dans l'espace de travail.

Les générateurs artistiques V3/V4 antérieurs replacent le départ à l'extérieur lorsqu'ils reconstruisent leur version. Pour réappliquer la maison après un tel rebuild, relancer `MaisonAccueilBuilder.Build()` en dernier.
