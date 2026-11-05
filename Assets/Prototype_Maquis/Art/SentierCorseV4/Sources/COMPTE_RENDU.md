# CorseVR — Grands arbres, maisons de passage et sentier littoral

Version du 23 octobre 2026, scène `Assets/Scenes/Prototype_Maquis.unity`, Unity 6000.6.7f1.

## Changements réalisés

- 33 grands arbres répartis en groupes irréguliers le long du maquis et à l'arrière des arbustes, en remplacement des anciens arbres de couverture. Hauteurs mesurées : 9,3 à 14,9 m. Trois variantes de chênes et deux de pins, créées dans Blender avec deux niveaux de détail, couronnes asymétriques, hauteurs et orientations variables. Huit pins supplémentaires sont placés côté terre sur le littoral.
- L'arche isolée est intégrée à une petite maison en pierre : façade et pignon maçonnés, petites ouvertures, volets gris-vert, porte ouverte en bois, toiture de tuiles canal et cheminée. Une seconde maison accueille le retour depuis le littoral. Les colliders des murs laissent le passage central libre.
- La plage provisoire est désactivée. L'arrivée mène à un sentier côtier sinueux de 33,9 m sur un terrain surélevé d'environ 5 à 6 m, avec mer à droite en contrebas, maquis bas, herbes et rochers sur les côtés. Cinq nouvelles formes de granite érodé ont été fabriquées dans Blender.
- Le chemin du maquis de 35,4 m, ses virages, ses limites et les repères d'interaction restent en place. Les passages fonctionnent dans les deux sens, dans la même scène. L'aperçu visible à travers la porte montre le nouvel environnement côtier.
- Les anciennes scènes et versions artistiques sont conservées. Copie de la scène avant modification : `Prototype_Maquis_avant.unity` dans ce dossier.

## Vérifications effectuées dans Unity

Tests en mode Play avec le contrôleur de déplacement réel, appels de marche par pas de 0,02 s et contrôles de passage :

| Vérification | Résultat |
| --- | --- |
| Parcours complet du maquis puis entrée dans la maison | Réussi ; 590 pas de simulation ; écart de suivi maximal 0,060 m |
| Arrivée littoral et modification du corridor | Réussi ; largeur latérale maximale configurée à 0,78 m de chaque côté |
| Parcours complet du littoral, retour au départ côtier et passage vers le maquis | Réussi ; 1 082 pas de simulation ; écart maximal 0,060 m |
| Appui au sol sur le littoral | Écart vertical maximal 0,031 m |
| Tentative de sortie latérale à cinq endroits dans chaque environnement, des deux côtés | Maquis : 0,7600002 m ; littoral : 0,7800102 m, tolérance numérique normale |
| Console à la fin des tests | Aucun avertissement, aucune erreur |
| État final | Hors mode Play ; scène enregistrée ; joueur au départ ; caméra à 1,65 m ; ancienne plage inactive |

Les captures `Apres/01` à `Apres/08` proviennent du rendu Unity à hauteur des yeux, en 1920 × 1080. Il ne s'agit pas d'images de concept.

## Sources et reprise

- Modèles modifiables : `Grands_Arbres_Corse.blend` dans ce dossier, 5 variantes d'arbres et 5 variantes de rochers ; FBX à deux niveaux de détail pour les arbres.
- Générateur : `Assets/Prototype_Maquis/Art/Editor/CorseLandscapeV4.cs`, méthode `Build()` en mode édition dans Prototype_Maquis. S'appuie sur le parcours V3 existant et reconstruit seulement son groupe V4. Le générateur V3 antérieur reconstruit l'ensemble de la scène et enlèverait les ajouts V4 : pour reprendre cette version, utiliser le générateur V4.
- Route côtière : `Assets/Prototype_Maquis/Scripts/CoastalRouteDefinition.cs`.
- Tests effectués : `Assets/Prototype_Maquis/Art/Editor/LandscapeValidationV4.cs`.
- Groupe principal : `Maquis_Environment/Sentier_Zigzag_Portail_Littoral/06_Grands_Arbres_Maisons_Cote_V4`.

## Limites artistiques et techniques

Le rendu reste un prototype stylisé. La végétation ancienne du maquis reste en grande partie présente ; le détail du feuillage, les matériaux du bâti, les sols et les raccords de terrain peuvent encore gagner en naturel. La maison est une interprétation de bâti rural corse, sans reproduction d'un édifice identifié. La côte est une composition originale, sans reconstitution géographique exacte.

Les tests ont été faits dans l'éditeur sur ordinateur. Pas de validation des performances, de la stéréoscopie des portes ni de la navigation sur Meta Quest 3 ; WebXR reste à intégrer/tester. Les scènes d'accueil, montagne, guide, interactions botaniques et sons ne sont pas ajoutées par cette modification.

## Références de direction artistique

- [Inventaire du patrimoine : maisons corses, granite et couverture traditionnelle en tuiles](https://pop.culture.gouv.fr/notice/merimee/IA2A000422).
- [Musée de la Corse : dossier pédagogique architecture et patrimoine, maison corse et bergerie](https://www.museudiacorsica.corsica/wp-content/uploads/2022/03/Dossier-pedagogique-2022.pdf).
- [Ministère de la Culture : Construire et restaurer en Haute-Corse, petites ouvertures, menuiseries et matériaux](https://www.culture.gouv.fr/content/download/315140/pdf_file/Guide%20UDAP%202B_Construire%20et%20restaurer%20en%20Haute-Corse.pdf?inLanguage=fre-FR&version=2).
- [Office de tourisme de Bonifacio : sentier de Fenu, maquis et reliefs du littoral](https://bonifacio.fr/a-voir-a-faire/balades-et-decouvertes/sentier-de-fenu/).
- [Office de tourisme de Porto-Vecchio : sentier littoral de la Testa–Ventilegna](https://www.portovecchio-tourisme.corsica/en/hiking-trail/sentier-du-littoral-de-la-testa-ventilegna/).
