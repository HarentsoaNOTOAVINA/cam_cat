# CAMT Parser avec harmonisation LLM

Ce projet est une application console .NET Core qui analyse les fichiers CAMT (relevés bancaires au format XML) et utilise un modèle de langage (LLM) pour harmoniser les libellés des transactions.

## Fonctionnalités

- Extraction des données de transactions à partir de fichiers CAMT.053
- Récupération des montants, dates, références et libellés
- Harmonisation des libellés via un LLM pour une meilleure lisibilité
- Affichage comparatif des libellés avant/après harmonisation

## Prérequis

- .NET 9.0 
- Accès à une API LLM (Gemini)

## Installation

1. Clonez ce dépôt :
   ```
   git clone https://github.com/votre-utilisateur/camt-parser.git
   cd camt-parser
   ```

2. Copie `.env.example` et changez les valeurs de votre configuration

## Utilisation

### Avec Visual Studio
1. Ouvrez la solution dans Visual Studio
2. Appuyez sur F5 ou cliquez sur "Démarrer" pour exécuter l'application

### Avec la ligne de commande
```
dotnet run
```

## Structure du code

- `Program.cs` : Point d'entrée principal avec la logique de l'application
    - `ExtractTransactionsFromCamt()` : Parse le fichier XML CAMT
    - `DisplayResults()` : Affiche les résultats dans la console
- `LlmService.cs` : Service pour interagir avec l'API LLM
    - `HarmonizeLabelsWithLlm()` : Envoie les libellés au LLM pour harmonisation
    - `GetHarmonizedLabelFromLlm()` : Gère l'appel API au LLM
- `Transaction` : Classe pour stocker les données de chaque transaction

## Notes

Ce projet est conçu comme une preuve de concept. Pour une utilisation en production, considérez les améliorations suivantes :
- Ajout de tests unitaires
- Gestion d'erreurs plus robuste
- Configuration externalisée (appsettings.json)
- Traitement par lots des appels API LLM
- Journalisation structurée

## Perspective (autre solution)
Ce projet pourrait ne pas dépendre du script qui fait l'extraction depuis le fichier xml.
Pour avoir plus de maintenabilité, je pense, ou pour avoir plus de fluidité dans le traitement de données (surtout ci ces dernieres sont massives), il faut utiiliser un outil ETL (Talend Open Studio) , et ensuite traté avec Llm les données traitées depuis Talend par exemple. 
- Talend Open Studio

## Licence
MIT