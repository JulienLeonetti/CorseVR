# CorseVR — sentier en zigzag et portail littoral

Révision du 23 octobre 2026. La scène enregistrée est Assets/Scenes/Prototype_Maquis.unity, dans le projet CorseVR. Elle est laissée hors mode Play, avec le joueur au départ.

## Résultat

Le parcours est remplacé par un tracé courbe de 35,42 mètres, avec plusieurs changements de direction. Il mène du départ à une clairière dégagée et à une arche de granit. Les limites de déplacement suivent le nouveau tracé ; le centre du joueur peut se décaler de ±0,76 m autour de l'axe.

Le maquis est planté sur plusieurs épaisseurs : bordures basses, arbustes proches, second rideau plus haut et arbres de couverture. 515 instances d'arbustes sont regroupées en maillages avec deux niveaux de détail, complétées par des arbres et des herbes. Les talus ferment les perspectives basses derrière les plantations. Les vues de contrôle se font à hauteur des yeux, et non uniquement depuis le dessus. Le remplissage est nettement plus important ; la finesse du feuillage reste celle des modèles stylisés des versions précédentes.

La fin est dégagée de végétation. La distance minimale entre le centre du portail et un sommet de végétation effectivement utilisé par les maillages est de 4,13 m au dernier contrôle. Le paramètre de placement est de 4,7 m, avec une marge de rejet adaptée au volume estimé des plantes ; la mesure de géométrie est la valeur à retenir pour l'espace réellement libre.

L'arche affiche une vue rendue de la zone littorale. Marcher à travers transporte le joueur vers une première plage avec sable, mer turquoise animée et rochers. Une arche de retour permet de rejoindre le maquis. Les deux zones sont dans la même scène, éloignées l'une de l'autre ; aucun changement de scène ni modification de Build Settings n'est nécessaire. Les prévisualisations sont des RenderTextures et deux caméras désactivées, rafraîchies à proximité du joueur. Leurs surfaces sont exclues des caméras de prévisualisation pour empêcher les récursions.

Le littoral ajouté est une zone d'arrivée jouable de prototype. Son aménagement artistique complet reste à réaliser. L'aperçu du portail est monoscopique ; ce n'est pas encore un portail à perspective stéréoscopique calculée depuis chaque œil.

## Vérifications

- Mode Play, avec appels de la méthode de déplacement du véritable DesktopFirstPersonExplorer : traversée du sentier jusqu'à 35,10 m, puis franchissement du portail.
- Douze tentatives de sortie latérale, à 0, 7, 13, 19, 26 et 32 m : toutes restent dans le corridor ; écart maximal mesuré 0,7601 m.
- Passage maquis → littoral validé. Arrivée à environ (220 ; 0,14 ; -1,80), corridor littoral configuré à ±1,10 m.
- Déplacement sur la plage jusqu'à environ huit mètres le long de son parcours validé.
- Retour littoral → maquis validé après correction du délai anti-rebond. Ce délai utilise l'horloge réelle pour rester fiable lorsque l'éditeur est en arrière-plan. Le corridor du maquis est rétabli à ±0,76 m.
- Caméra du joueur à 1,65 m, une seule caméra taguée MainCamera et un seul AudioListener.
- Shader d'eau pris en charge, sans message de compilation. Dernière console : zéro erreur et zéro avertissement retournés.
- Scène enregistrée, sans modification en attente et sans compilation en cours.

Ces contrôles vérifient le contrôleur, la géométrie du parcours et les transitions. Ils ne remplacent pas un essai manuel au clavier ni un test sur casque. Les performances et la locomotion WebXR sur Quest 3 ne sont pas validées. Le volume de végétation, les ombres et les prévisualisations nécessiteront une mesure et une adaptation au casque.

## Organisation actuelle

Le groupe actif est Maquis_Environment/Sentier_Zigzag_Portail_Littoral. Les versions antérieures sont désactivées et leurs assets sont conservés. Les repères de népita et de la vieille femme sont déplacés près du nouveau parcours ; aucun dialogue ni son n'est ajouté.

Le constructeur actuel est CorseVR.Editor.MaquisZigzagBuilder.Build. Les constructeurs BavellaJourneyBuilder, SentierCorseBuilder et SentierCorseFinition décrivent des versions antérieures ; ne pas les relancer pour continuer cette version.

Sources ajoutées :

- Assets/Prototype_Maquis/Scripts/MaquisRouteDefinition.cs — définition du nouveau tracé.
- Assets/Prototype_Maquis/Scripts/BiomePortal.cs — franchissement, destination et temporisation.
- Assets/Prototype_Maquis/Scripts/PortalPreview.cs — rafraîchissement de la vue du portail.
- Assets/Prototype_Maquis/Art/Editor/MaquisZigzagBuilder.cs — terrain, végétation, arches et arrivée littorale.
- Assets/Prototype_Maquis/Art/SentierCorseV3/ — nouveaux maillages, matériaux, RenderTextures et shader d'eau.

DesktopFirstPersonExplorer reçoit une méthode de remise à zéro de la vitesse verticale lors d'un passage de portail. Les commandes ZQSD/flèches, souris, Échap et clic restent les mêmes. Les arbres et arbustes réutilisent les kits Blender précédents. Les transformations de conversion d'unités FBX sont conservées sous un objet parent pour les rochers.

## Livrables

Apres/ contient sept captures Unity natives 1920 × 1080 : départ, premier virage, côtés gauche et droit, clairière du portail, littoral et vue générale du parcours. Six sont à hauteur des yeux ; la dernière est une vue de dessus pour contrôler l'ensemble. Aucune retouche d'image.

Validation_Parcours.json conserve les tests du sentier et du premier passage. Test_Portail_Aller.json et Test_Portail_Retour.json conservent les derniers tests des transitions, après correction de la temporisation. Etat_Final.json contient le relevé de sauvegarde et de dégagement de la clairière. Prototype_Maquis_avant.unity est une copie de la scène avant cette révision, à utiliser avec les assets antérieurs conservés dans le projet.

Aucun modèle tiers, achat ni téléchargement d'asset ajouté pendant cette passe.
