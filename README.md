# CamtParser - Test Technique

Application console .NET pour parser des fichiers CAMT.053 et harmoniser les libellés bancaires via LLM.

## Choix Techniques

### 1. Parsing XML

Nous utilisons `XDocument` (LINQ to XML) pour une extraction flexible.
**Point Clé :** L'extraction de la référence transactionnelle a été ajustée pour cibler `<AcctSvcrRef>` (Account Servicer Reference) conformément au standard SEPA standard et au guide fourni, plutôt que `<InstrId>` qui n'est pas toujours présent ou pertinent pour le relevé client.

### 2. Service LLM et Optimisation (Batching)

L'interaction avec le LLM (Google Gemini) est optimisée par **batching**.

- **Problème initial :** Traiter les transactions une par une est inefficace (n requêtes HTTP pour n transactions) et risque de dépasser les quotas API.
- **Solution :** Les transactions sont regroupées par lots (taille de 10) et envoyées en une seule requête avec un prompt structuré demandant une réponse JSON.
- **Avantages :**
  - Vitesse de traitement drastiquement améliorée.
  - Réduction de la consommation de quota (nombre d'appels divisé par 10).
  - Meilleure cohérence contextuelle pour le modèle.

## Difficultés et Résolutions

- **Format de Réponse LLM :** Les modèles génératifs peuvent être verbeux.
  _Solution :_ Utilisation d'un prompt strict ("JSON only") et nettoyage de la réponse (suppression des balises markdown) pour garantir un parsing sans erreur.
- **Fiabilité XML :** Les fichiers CAMT peuvent varier.
  _Solution :_ Extraction défensive avec `?.` et fallbacks (ex: libellé pris dans `Ustrd`, sinon `AddtlTxInf`).

## Améliorations Futures

- **Configuration :** Utiliser `appsettings.json` et `IOptions<T>` au lieu d'un reader personnalisé.
- **Tests :** Ajouter des tests unitaires pour le parser XML avec différents cas de figures (fichiers malformés, champs manquants).
- **Polymorphisme :** Abstraire le service LLM derrière une interface `ILabelHarmonizer` pour faciliter le changement de provider (OpenAI, Mistral...).

## Instructions

1.  Configurer l'API Key dans `.env` ou `AppConfig`.
2.  Placer le fichier CAMT source.
3.  Lancer : `dotnet run`
