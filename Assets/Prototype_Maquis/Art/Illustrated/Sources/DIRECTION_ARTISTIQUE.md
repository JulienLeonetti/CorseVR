# CorseVR — Maquis illustré, première zone de référence

Scène : `Assets/Scenes/Prototype_Maquis.unity`. Version enregistrée le 22 octobre 2026 dans Unity 6000.6.7f1, hors mode Play.

## Direction artistique réalisée

Les vingt premiers mètres sont composés comme une entrée dans un paysage illustré : premier plan encadré, chemin ocre lumineux, passages végétaux plus denses, petites respirations latérales et collines étagées par la brume. La vue arrière est également traitée.

Les silhouettes arrondies et les courbes des branches interprètent la douceur expressive évoquée par More Sugar. Les feuillages ont des masses pleines et irrégulières, des feuilles de silhouette et des variations de teinte. Le sol, les pierres et les plantes utilisent des matériaux mats à variations picturales ; les détails photographiques de l’étude précédente ne sont plus utilisés par la scène.

La palette associe sauge, olive, verts plus profonds, terre ocre, pierres chaudes et ciel bleu doux avec nuages clairs. Un seul soleil chaud structure les volumes et les ombres restent lisibles. Le relief accompagne le chemin, dont la largeur et les bordures varient. Le raccord avec les collines a été fermé, y compris derrière le départ.

La composition contient 98 arbustes de décor, six petits arbres aux formes d’arbousiers, 265 touffes, 68 tiges fleuries et 98 petites pierres, en plus des huit rochers avec collisions. Ces plantes évoquent lentisque, ciste et bruyère. Elles constituent un vocabulaire de décor stylisé, pas encore une collection de modèles botaniques pédagogiques validés.

## Espaces préparés

Deux petites clairières sont accessibles à pied depuis le chemin :

- `Emplacement_Nepita` : x=-4,9 ; z=-4,6.
- `Emplacement_Vieille_Femme` : x=3,8 ; z=1.

Les repères `Son_Directionnel_Gauche` et `Son_Directionnel_Droit` préparent la suite. Aucun personnage, interface, système de sélection ou son n’a été ajouté pendant ce travail de décor. Les repères sont des objets vides sans affichage dans l’expérience.

## Ressources et sources

Les maillages, silhouettes végétales, fleurs, ciel et shaders de cette version sont des créations procédurales originales réalisées pour CorseVR. Le générateur travaille uniquement dans l’éditeur ; la scène charge ensuite des maillages et matériaux enregistrés. Il n’y a aucun générateur de décor exécuté en continu pendant l’exploration, aucun nouveau téléchargement et aucune police More Sugar importée.

Source reproductible : `Assets/Prototype_Maquis/Art/Editor/MaquisIllustratedBuilder.cs`.

Ressources actives : `Assets/Prototype_Maquis/Art/Illustrated/`.

Menu : `CorseVR > Build illustrated maquis reference`. La régénération remplace le groupe `Maquis_Illustre_Reference`, les surfaces du sol et des rochers, et préserve le joueur. Les changements manuels dans le groupe généré seront remplacés si ce menu est exécuté.

L’étude naturaliste précédente et ses sources restent conservées dans les fichiers du projet. Son générateur est rangé sous `CorseVR > Archive > Rebuild previous natural study`. Ses textures Poly Haven sont conservées avec leurs URL, licences et empreintes dans `Art/Sources/MAQUIS_SOURCES.md` et `Art/Sources/manifest.json`. Elles ne sont plus référencées par la scène active : aucune dépendance vers `Art/Textures/` lors du contrôle final.

Pour mémoire, les ensembles archivés sont [Rocky Trail](https://polyhaven.com/a/rocky_trail) d’Amal Kumar, [Brown Mud 02](https://polyhaven.com/a/brown_mud_02) de Rob Tuytel et [Rock Boulder Dry](https://polyhaven.com/a/rock_boulder_dry) de Dimitrios Savva et Rico Cilliers. Leur [licence CC0](https://polyhaven.com/license) et les attributions restent conservées. Aucun modèle tiers n’a été importé pour ce passage illustré.

## Contrôles effectués

Essais déterministes en mode Play avec le contrôleur existant, son système d’entrée et les collisions Unity :

- ZQSD selon la disposition du clavier, quatre flèches, vitesse diagonale normalisée : réussis.
- Déplacement de trois mètres en une seconde à la vitesse existante : réussi.
- Regard à la souris, rotation horizontale et limite verticale : réussis.
- Échap libère et rend visible le curseur : réussi.
- Gravité et appui au sol au départ : réussis. La caméra conserve sa hauteur locale de 1,65 m ; la marge du CharacterController ajoute environ 3 cm pendant la simulation.
- Parcours complet par cinq points du chemin, de z=-11 à z=11 : réussi, sans obstruction ni perte de sol.
- Huit rochers : chaque collision de maillage arrête le joueur à l’approche.
- Accès depuis le chemin à la clairière nepita et à celle de la future rencontre : réussis.
- Console finale : zéro erreur et zéro avertissement.
- Shaders actifs et ciel : aucun message de compilation.
- Retour hors Play et scène enregistrée.

Le fichier du contrôleur et SampleScene sont inchangés, vérifiés par SHA-256.

## Budget observé sur ordinateur

Comptage de l’ensemble des géométries actives, horizon compris : **148 438 triangles, 157 082 sommets, 49 renderers de maillage et trois matériaux partagés**. Le ciel utilise son propre matériau. La végétation et le décor sont opaques, sans cartes de feuillage transparentes. Les petites tiges ont une géométrie réduite. Un seul soleil, des groupes spatiaux et des shaders cohérents limitent les coûts ; les réglages globaux du pipeline ont été préservés. Le post-traitement de la caméra est désactivé.

Ces nombres sont des comptages de scène, pas des mesures de draw calls ou de fréquence d’image. La compilation WebXR/WebGL, la prise en charge des shaders dans le navigateur, les coûts de la stéréoscopie et les performances réelles du Quest 3 restent à tester sur appareil.

## Captures livrées

Captures Unity natives, 1280 × 800, FOV du contrôleur inchangé à 70°, sans retouche d’image. Chaque caméra est à 1,65 m au-dessus du terrain local :

1. `CorseVR_01_Depart.png` : (-3,00 ; 1,65 ; -11,00), vers le chemin.
2. `CorseVR_02_Chemin.png` : (-0,99 ; 1,80 ; -1,80), vers la suite du parcours.
3. `CorseVR_03_Cote.png` : (-2,18 ; 1,77 ; -4,80), vue latérale depuis le chemin, légèrement inclinée vers la clairière.

Une quatrième vue vers l’arrière a servi au contrôle des raccords et de la composition. La caméra de jeu a été remise à sa position et son orientation de départ après les captures.

## Limites pour la validation artistique

La pièce jointe reçue contenait uniquement le texte, sans l’image de référence mentionnée. L’interprétation repose donc sur la description artistique et la palette demandées. La validation botanique des futurs spécimens interactifs, les contenus narratifs et les mesures en casque constituent les étapes suivantes.
