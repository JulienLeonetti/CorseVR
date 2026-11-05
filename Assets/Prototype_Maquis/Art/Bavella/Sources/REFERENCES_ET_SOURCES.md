# CorseVR — références et provenance du parcours maquis / Bavella

## Photographies effectivement examinées

Ces photographies ont servi à l'observation. Elles ne sont ni intégrées aux textures, ni redistribuées dans le projet. Aucune licence de réutilisation de ces photographies n'est présumée.

| Source | Référence | Éléments traduits dans le décor |
|---|---|---|
| Canyon Corse, galerie de Bavella | [Page](https://www.canyoncorse.com/fr/canyon-gr20-aiguilles-bavella-montagne-corse_9.html), [photographie examinée](https://www.canyoncorse.com/images/20_orig.jpg) | Passage entre blocs, arène et pierres de tailles variées, fissures du granit, pins à branches étagées, continuité avec la montagne lointaine. |
| Photos Paysages Corse, Bavella | [Galerie](https://photos-paysages-corse.com/page_Bavella.htm), [photographie 43 examinée](https://photos-paysages-corse.com/images/Bavella%2043.JPG) | Sentier étroit dans un maquis dense, feuillage ajouré, relief qui dépasse déjà la végétation, ouverture sur les aiguilles. |

Consultation : 22 octobre 2026. Les images ont été examinées visuellement dans le navigateur. Les liens sont conservés pour retrouver les originaux.

## Géographie et botanique

- [Office du tourisme de Corse — Aiguilles de Bavella](https://www.visit-corsica.com/fr/Mon-sejour/Patrimoine-naturel/Tout-le-patrimoine-naturel/LES-AIGUILLES-DE-BAVELLA) : silhouettes déchiquetées, murailles granitiques et pins déformés par le vent. Traduction : sept formations regroupées avec des hauteurs différentes, contreforts, plans lointains et pins. Le décor est une évocation scénographique condensée, sans reproduction topographique ou cartographique.
- [Sorbonne Université, Biologie et Multimédia — Lentisque, Roger Prat et Jean-Pierre Rubinstein](https://www.snv.jussieu.fr/bmedia/arbres/lentisque.htm) : feuillage composé portant des paires de folioles. Traduction : ramifications étalées, rachis avec quatre paires de folioles sans foliole terminale, vert olive.
- [Tela Botanica — Arbutus unedo](https://www.tela-botanica.org/bdtfx-nn-6055) : feuillage persistant, feuilles larges et dentées, jeunes rameaux rougeâtres. Traduction : port plus haut, ramifications brun-rouge et feuilles larges à bord irrégulier. L'identification botanique reste une interprétation stylisée ; aucune validation scientifique des modèles n'est revendiquée.
- [Tela Botanica — Cistus monspeliensis](https://www.tela-botanica.org/bdtfx-nn-18026-synthese) : feuilles étroites, port bas et fleurs blanches. Traduction : cistes bas des lisières ensoleillées, feuilles opposées étroites et petites fleurs à cinq pétales. Les trois nervures et la texture visqueuse ne sont pas reproduites avec précision.
- [Col de Cricheto — sentier découverte du maquis](https://www.colcricheto.com/Sentier-Decouverte-du-Maquis_a9.html) : référence de parcours corse associant découverte de plantes et points de vue. Consultation documentaire ; la forêt de châtaigniers de cette page n'a pas été copiée dans la scène.

Les sources botaniques sont utilisées pour la morphologie. Leur texte, leurs photographies et leurs illustrations n'ont pas été copiés dans les assets.

## Modèles originaux

L'inventaire initial ne contenait aucun modèle végétal ou rocheux exploitable sous Assets : les sept modèles détectés appartenaient aux ressources de diagnostic des packages Unity.

Modèles créés localement avec Blender 5.2.3 LTS, à partir du script build_corsican_kit.py :

- Trois variantes de lentisque, trois d'arbousier et trois de ciste.
- Trois pins inspirés des pins de Bavella.
- Deux niveaux de détail par végétal, avec silhouettes, branches et feuilles en géométrie opaque.
- Cinq blocs granitiques, sept formations d'aiguilles et un petit galet.
- Total : 37 fichiers FBX, puis douze prefabs végétaux avec LOD dans Unity.
- Source éditable complète : CorseVR_Corsican_Kit.blend, livrée à côté de ce document.
- Script source et statistiques également conservés dans Assets/Prototype_Maquis/Art/Bavella/Sources/.

Aucun modèle tiers téléchargé. Aucun coût de licence tiers ajouté par cette refonte. Les archives de sources et de licences des versions antérieures restent conservées ; les anciens groupes de décor sont désactivés.

## Textures et matériaux

Granit_Bavella_Peint.png est une texture originale créée avec la compétence imagegen, mode intégré image_gen, puis importée dans Unity. Source conservée intacte, import plafonné à 1024 × 1024, mipmaps, répétition et anisotropie 4, réglage WebGL ASTC 6×6.

Fichier de projet : Assets/Prototype_Maquis/Art/Bavella/Textures/Granit_Bavella_Peint.png.
Copie livrée : Granit_Bavella_Peint.png dans ce dossier.

Prompt final :

> Use case: stylized-concept. Asset type: seamless tileable base-color texture for real 3D granite rock models in a stylized Corsican landscape inspired by Bavella. Primary request: hand painted warm pale rose and sandy grey Corsican granite, muted salmon feldspar patches, soft ivory quartz patches, tiny charcoal mica speckles, occasional muted sage lichen. Square top-down flat albedo texture. Painterly broad brushwork with fine restrained mineral grain, rounded soft shading variation within the paint. Consistent warm natural colors; pale cream 50%, dusty pink 25%, grey taupe 20%, charcoal 5%. No baked cast shadows, no directional lighting, no rock silhouette, no objects, no text, no border. Seamless all four edges, uniform scale. Do not draw large cracks: geometric fissures will be built in 3D.

L'écorce peinte de la précédente passe est réutilisée. Le feuillage reçoit des couleurs de sommets peintes, une nervure et des variations procédurales de valeur. Le sol mélange arène claire, terre et teintes de végétation avec un grain minéral. Les matériaux restent mats ; le feuillage est opaque et à double face. Les shaders conservent les ombres du soleil et le brouillard de profondeur.

