# CorseVR — refonte ciblée du parcours maquis / Bavella

## Résultat et direction artistique

Prototype_Maquis est enregistrée. Le départ mène à environ dix mètres de maquis, puis à un passage de granit et une ouverture sur un massif inspiré de Bavella. Le relief apparaît dès les premiers mètres ; le virage déplace le masque rocheux et les lisières s'ouvrent ensuite. La découverte principale se situe vers 14 mètres. Le tracé complet reste praticable sur environ 23 mètres.

Le repère des dix mètres est obtenu par longueur cumulée du tracé courbe depuis Depart_WebXR, plutôt que par une simple distance sur l'axe Z. Il se trouve vers (0,20 ; -2,30) dans le plan du sol. Le test avec le contrôleur atteint ce repère à quelques centimètres près, puis poursuit jusqu'à 22 mètres.

La nouvelle enveloppe comprend un terrain détaillé de 36 × 36 mètres, des collines périphériques sur 240 × 240 mètres, des affleurements latéraux, un arrière planté, des contreforts, sept formations principales d'aiguilles et des reliefs plus bleutés. Les montagnes sont des volumes en trois dimensions qui restent cohérents lorsque la caméra se déplace.

Le sentier reçoit des variations de terre et d'arène granitique, de petites irrégularités et des pierres partiellement enfouies. Les bordures associent cistes bas, lentisques étalés et quelques arbousiers plus hauts. Les pins deviennent plus présents vers le massif. La palette réunit verts olive, sauge et vert profond, granit rosé et lumière chaude, avec des ombres plus fraîches.

## Références effectivement traduites et qualité artistique

Les photographies de sentiers de Bavella ont guidé l'association de blocs fissurés, végétation ajourée et pins étagés. Une photographie du maquis devant les aiguilles a guidé la première vision du relief au-dessus des arbustes. Les descriptions botaniques ont guidé les paires de folioles du lentisque, les feuilles plus larges de l'arbousier et le feuillage étroit des cistes. Les liens exacts, les utilisations et la provenance des assets sont conservés dans REFERENCES_ET_SOURCES.md.

L'amélioration obtenue est une évocation corse plus précise que les anciens volumes génériques : les végétaux sont différenciés, le sol devient minéral et le massif constitue la destination visuelle. Les matériaux peints et les formes érodées conservent une certaine douceur.

La qualité artistique demeure celle d'un prototype stylisé. Les aiguilles évoquent Bavella sans reproduire exactement une silhouette géographique. Leurs fissures secondaires restent simplifiées ; les houppiers des pins conservent une construction répétitive et les herbes de fond restent sommaires. Les feuilles, bien que ramifiées et différenciées, gagneraient encore à recevoir une peinture propre à chaque espèce. La finesse picturale et la densité de la référence initiale ne sont pas atteintes partout. Les emplacements d'interaction sont réservés, sans ajout des futures interactions.

## Ressources et organisation

Le kit original a été créé dans Blender 5.2.3 LTS puis importé dans Unity : trois variantes par espèce d'arbuste, trois pins, cinq blocs, sept formations d'aiguilles et un galet. Les végétaux possèdent chacun deux niveaux de détail : 37 FBX et douze prefabs végétaux au total. La scène contient 51 lentisques, 7 arbousiers, 71 cistes et 48 pins, ainsi que 71 formations rocheuses distribuées entre le proche et l'horizon.

Les noms High / Low des fichiers importés évitent les avertissements d'auto-détection LOD ; les LODGroup Unity sont configurés explicitement. Les troncs de pins ont été corrigés pour supprimer les raccords de diamètre visibles.

Le groupe actif est Maquis_Environment/Parcours_Corse_Bavella, organisé en terrain, végétation, granit proche, massif et préparation des interactions. Les anciens groupes de décor sont conservés désactivés. Les nouveaux assets sont sous Assets/Prototype_Maquis/Art/Bavella. Le générateur est sous Art/Editor/BavellaJourneyBuilder.cs.

Dix matériaux de surface sont partagés, plus le ciel. Les feuilles sont en géométrie opaque à double face, ce qui évite de multiplier les couches transparentes. La texture de granit originale est limitée à 1024 × 1024 à l'import ; les sources, le prompt image_gen et la source Blender sont livrés. Aucun nouveau modèle tiers n'a été téléchargé.

## Validation technique

- Contrôleur existant conservé, script inchangé : ZQSD selon la disposition clavier, flèches et diagonales normalisées.
- Vitesse contrôlée à 3 m/s ; regard souris et limitation verticale vérifiés ; Échap libère la souris.
- Gravité et appui au sol vérifiés. Le contrôleur a parcouru le chemin sans téléportation entre ses étapes, de 0 à 22 mètres.
- Emplacement_Nepita et Emplacement_Vieille_Femme restent présents et accessibles depuis le chemin.
- 52 MeshColliders actifs correspondent aux terrains et rochers ; les troncs ont leurs colliders dédiés. Le CharacterController est arrêté par les deux rochers testés.
- Aucun mesh manquant, shader pris en charge et sans message de compilation. Contrôleur actif dans la scène finale.
- Dernière console : zéro erreur, un avertissement de transport unityMCP concernant une commande renvoyée puis dédupliquée. Les avertissements de nommage LOD ont été corrigés.
- SampleScene et DesktopFirstPersonExplorer.cs conservent leurs empreintes SHA-256 initiales. La scène finale comporte quatre racines et ne porte aucune modification non enregistrée.

Les résultats détaillés sont dans Validation_Controles.json et Scene_Evidence.json.

## Performances observées sur ordinateur

Mesure en Play mode dans Unity 6000.6.7f1, Direct3D 12, RTX 4050 Laptop, Ryzen 7 7735HS, environ 16 Go de RAM. Vue Game réelle de 1920 × 1080, Render Scale 1, MSAA 4×, VSync désactivée. Pour chaque vue : 45 images de stabilisation puis 180 images mesurées. Le rendu en arrière-plan est autorisé pendant la mesure puis remis à sa valeur précédente.

Réglage final : ombres sur deux cascades à 32 mètres dans le profil dédié Maquis_Desktop_MSAA4. Le profil PC d'origine est préservé.

| Vue | Temps moyen (ms) | P95 (ms) | Cadence moyenne (i/s) | GPU moyen relevé (ms) |
|---|---:|---:|---:|---:|
| Depart 0m | 25.77 | 33.51 | 38.8 | 7.82 |
| Milieu 5m | 18.36 | 21.55 | 54.5 | 6.69 |
| Transition 10m | 16.48 | 22.77 | 60.7 | 5.36 |
| Decouverte 14m | 17.01 | 38.01 | 58.8 | 4.36 |
| Lateral 6m | 13.71 | 19.51 | 73.0 | 4.70 |

La cadence observée dans cette dernière passe va donc d'environ 39 à 73 images/s et reste variable. Les premières passes étaient plus rapides. Il n'est pas possible d'en déduire une cadence garantie en exécutable ; l'éditeur, ses autres vues et l'activité de l'ordinateur influencent la mesure. Le JSON complet conserve aussi les pics.

Le relevé avant réduction des ombres est conservé dans Performances_Desktop_Ombres4Cascades.json. Les appels de rendu rapportés par l'éditeur au départ passent de 1091 à 771, mais les temps d'image ne permettent pas d'affirmer un gain de cadence contrôlé entre ces deux passes.

Le kit reste coûteux au niveau maximal : les instances végétales réunies représentent théoriquement 7,08 millions de triangles avec tous les modèles détaillés, contre environ 688 000 avec tous les modèles allégés. Ces totaux ne décrivent pas les seuls objets visibles ni le nombre de triangles d'une image, mais montrent la nécessité de conserver le culling et les LOD. Les compteurs UnityStats bruts sont inclus dans le JSON.

## Vérifications encore nécessaires pour Quest 3

La version WebXR n'a pas été construite ni mesurée sur casque. Les assets sont organisés pour pouvoir adapter le profil, mais ce prototype ne constitue pas une validation de fluidité Quest 3.

Il reste à mesurer un build WebXR stéréo dans le navigateur du Quest 3 : temps CPU/GPU, cadence soutenue après échauffement, mémoire et téléchargement, fonctionnement des shaders en stéréo, rendu des ombres et transitions LOD. Le budget des modèles proches et des ombres devra probablement être réduit davantage. Il faudra également vérifier les déplacements VR et le confort indépendamment du contrôleur ordinateur.

## Captures livrées

Toutes les images sont des rendus Unity natifs de 1920 × 1080, caméra à 1,65 m au-dessus du sol, champ vertical de 65°, MSAA 4×. Elles n'ont été ni agrandies ni retouchées. Les poses sont conservées dans Scene_Evidence.json.

1. 01_Depart_Maquis.png — départ, première vision partielle du massif.
2. 02_Milieu_5m.png — chemin enveloppé et passage rocheux qui se précise.
3. 03_Transition_10m.png — sortie du maquis et ouverture du panorama.
4. 04_Decouverte_Montagne.png — découverte vers 14 mètres.
5. 05_Alentours_Lateral.png — traitement des côtés à hauteur des yeux.
6. 06_Retour_Arriere.png — contrôle complémentaire de l'arrière du départ.

La source éditable CorseVR_Corsican_Kit.blend et le script build_corsican_kit.py sont joints à ce dossier.

