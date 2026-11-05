# Prototype_Maquis — diagnostic du rendu et feuillage

## Comparaison visuelle

La référence fournie mesure 1922 × 887 pixels. Elle combine des masses de végétation lisibles à distance, des rameaux intermédiaires, de petites feuilles peintes et des ouvertures entre les branches. Les anciennes captures du prototype (1280 × 800, et auparavant 871 × 367) montraient surtout des volumes pleins : contours facettés, peu de vides et une faible variété de silhouettes.

L'amélioration est concentrée sur les premiers mètres du chemin : 20 arbustes et 2 arbres. Les volumes arrondis, la palette chaude et le paysage de maquis sont conservés. Le fond demeure plus simple ; la richesse globale de la référence n'est pas encore reproduite sur toute la scène.

## Diagnostic mesuré dans Unity

| Paramètre | Avant | Après |
|---|---|---|
| Cible et texture réelle de la vue Game | 3840 × 2160 | 1920 × 1080 |
| Zoom de l'aperçu Game | 0,2126, affiché 0,21× | environ 0,4252, affiché 0,43× |
| Échelle de l'interface | 1,25 / 125 % | inchangée |
| URP Render Scale | 1 | 1 |
| MSAA du pipeline actif | 1 échantillon : désactivé | 4 échantillons |
| Camera.allowMSAA | vrai, mais pipeline sans AA | vrai, pipeline MSAA 4× |
| Anticrénelage de post-traitement | aucun | aucun |
| Captures livrées | anciennes images 1280 × 800 | nouvelles images natives 1920 × 1080 |

La vue Game initiale n'était donc pas rendue en basse résolution : elle calculait une image 4K réduite dans l'éditeur. Les anciennes captures étaient plus petites et l'anticrénelage était réellement désactivé. Les grandes cassures des volumes de feuillage provenaient aussi de leur géométrie ; augmenter la résolution ne pouvait pas les supprimer.

Le zoom observé concerne l'aperçu de la vue Game. Le code officiel de Unity distingue sa taille cible et le zoom d'affichage : [GameView.cs](https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/GameView/GameView.cs). La sélection HD a été appliquée par le mécanisme de sélection de taille, puis la texture réelle a été relue pour vérifier 1920 × 1080.

Le profil actif est désormais Maquis_Desktop_MSAA4, copie dédiée du profil PC. Le profil PC d'origine est préservé. Un composant de scène applique ce profil et restaure le précédent lorsqu'il est désactivé. La résolution dynamique de la caméra est désactivée.

Les tampons internes de couleur ET de profondeur ont été observés en 1920 × 1080 avec le suffixe MSAA4x. La texture finale résolue comporte un seul échantillon, ce qui ne signifie pas que le MSAA est désactivé. Les mesures finales sont conservées dans CorseVR_Rendu_Evidence.json.

## Avant/après reproductible

- Avant_Vegetation_1920_AA4.png : végétation précédente avec résolution et AA corrigés.
- Apres_Vegetation_1920_AA4.png : végétation enrichie avec les mêmes réglages.
- Diagnostic_Avant_1920_SansAA.png : état initial sans AA, rendu lui aussi nativement en HD pour isoler ce défaut.

Les deux images comparatives utilisent exactement la même position caméra (-3 ; 1,65 ; -11), une rotation monde (0 ; 0 ; 0), un champ vertical de 70°, un ratio 16:9 et le même éclairage.

Chaque PNG provient directement d'un rendu Unity vers une RenderTexture de 1920 × 1080, puis d'une lecture complète des pixels. Aucun agrandissement, montage ou retouche d'image. Les images comparatives utilisent un tampon MSAA 4× et un Render Scale de 1.

## Feuillage et ressources

Les nouveaux arbustes et arbres comportent des branches courbes, des rameaux secondaires et 2793 bouquets de feuilles sur des surfaces courbées. Tailles, inclinaisons, positions, densités et teintes varient. Les petites fleurs de ciste apportent des accents clairs. Les feuilles basses sont écartées du corridor de marche.

Deux textures originales ont été créées avec le mode image_gen intégré, selon la compétence imagegen :

- Painted_Leaf_Spray.png : source RGBA 1254 × 1254, fond réellement transparent ; import Unity limité à 1024 × 1024.
- Painted_Bark.png : source RGB 1254 × 1254 ; import limité à 512 × 512.

Les fichiers source intacts sont livrés dans Textures/. Les prompts complets et les emplacements sont consignés dans CorseVR_Prompts_Textures.txt. Aucun nouveau modèle tiers n'a été ajouté ; les sources et licences précédemment archivées sont conservées.

Dans le projet, les textures et le shader se trouvent sous Assets/Prototype_Maquis/Art/FoliageReference/. Les matériaux utilisent une découpe alpha, l'écriture de profondeur et l'alpha-to-coverage avec MSAA. Les mipmaps préservent la couverture alpha du feuillage. Import WebGL : ASTC 4×4 pour les feuilles et 6×6 pour l'écorce.

## Validation et limites

Prototype_Maquis est enregistrée. SampleScene et le script de déplacement ont conservé leurs empreintes de fichier.

Les vérifications en mode Play ont confirmé ZQSD selon la disposition clavier et les flèches, vitesse et diagonales, regard souris, limitation verticale, libération de la souris par Échap, gravité et contact avec le sol. Le parcours du chemin et les collisions des huit rochers ont été vérifiés après modification.

Les shaders sont pris en charge et sans message de compilation. Le dernier contrôle de console ne retourne aucune erreur ni aucun avertissement. Deux avertissements de déduplication du transport unityMCP avaient été observés pendant les opérations, sans erreur de décor.

La scène active contient environ 157 988 triangles, 168 368 sommets, 76 MeshRenderers et cinq matériaux de surface partagés, plus le ciel. Ces chiffres décrivent la géométrie ; ils ne constituent pas une mesure du temps GPU.

La zone limitée, les textures plafonnées et les matériaux partagés maîtrisent la portée de l'ajout. Le coût du feuillage découpé et du MSAA reste à mesurer dans un build WebXR stéréo sur le navigateur du Quest 3. Aucun débit d'images sur casque n'est revendiqué.

