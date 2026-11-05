# Corrections de la maison — 23 octobre 2026

Scène : `Assets/Scenes/Prototype_Maquis.unity`.

- Suppression des deux plaques d'enduit qui s'intersectaient avec les pierres et produisaient les défauts visibles sur les captures.
- Suppression des bouquets d'herbes suspendus et de leurs attaches. La carte murale de la Corse est conservée.
- Ajout de menuiseries en bois à croisillons et de quatre carreaux par fenêtre dans les deux ouvertures existantes. Les vitres sont transparentes, légèrement teintées, sans ombre opaque. L'éclairage des fenêtres a été éloigné des cadres pour éviter les zones blanches surexposées.
- Marmite : fond fermé par une face pleine, intérieur distinct, reflets métalliques atténués et bûches abaissées sous le récipient pour supprimer leur pénétration dans le fond.

Les corrections ont été réalisées dans le modèle Blender, réimportées dans Unity et enregistrées. Les captures dans `Apres/` sont des rendus Unity, avec un gros plan supplémentaire de la marmite.

Vérifications : absence des anciennes plaques et des bouquets dans le modèle actif ; présence des vitres et du fond de marmite ; contrôle visuel des fenêtres et du récipient ; test en mode Play de l'interaction de porte, du blocage porte fermée et de la sortie porte ouverte. Tous les tests ont réussi. La console ne signalait aucune erreur ni aucun avertissement.

État final après enregistrement et rechargement : hors mode Play, joueur à l'intérieur, porte fermée, aucune modification non enregistrée. Les matériaux et objets des chemins et des portails ne sont pas modifiés par ces corrections.

Fichier Blender modifiable : `Maison_Corse_Corrigee.blend` dans ce dossier. Copie de scène avant intervention : `Prototype_Maquis_avant.unity`. Générateur actuel : `Assets/Prototype_Maquis/Art/Editor/MaisonArtistiqueBuilder.cs`, méthode `Build()`. Shader de vitrage : `Assets/Prototype_Maquis/Art/MaisonArtistique/WindowGlass.shader`.
