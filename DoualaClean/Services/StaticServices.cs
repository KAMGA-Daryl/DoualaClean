namespace DoualaClean.Services;

/// <summary>Bacs de collecte (données d'exemple : coordonnées approximatives à remplacer par l'API de la mairie/Hysacam).</summary>
public sealed class BinService : IBinService
{
    private readonly List<CollectionBin> _bins = new()
    {
        new CollectionBin
        {
            Id = 1, Name = "Bac Akwa - Rond-point Deïdo", Location = "Rond-point Deïdo / Carrefour Ancien Sapeur",
            WasteType = "Plastiques & Emballages", FillPercent = 85, NextCollection = "Demain à 06:00",
            Responsible = "Équipe de ramassage zone Nord", Latitude = 4.0636, Longitude = 9.7144, Image = "bin_hysacam.jpg"
        },
        new CollectionBin
        {
            Id = 2, Name = "Bac Akwa - Boulevard de la Liberté", Location = "Boulevard de la Liberté, Akwa",
            WasteType = "Déchets organiques", FillPercent = 60, NextCollection = "Après-demain à 06:00",
            Responsible = "Équipe de ramassage zone Centre", Latitude = 4.0511, Longitude = 9.7679, Image = "bin_street.jpg"
        },
    };

    public IReadOnlyList<CollectionBin> GetAll() => _bins;
    public CollectionBin? GetById(int id) => _bins.FirstOrDefault(b => b.Id == id);
}

public sealed class SortingGuideService : ISortingGuideService
{
    private readonly List<SortingCategory> _items = new()
    {
        new SortingCategory
        {
            Key = "organic", Title = "Déchets organiques", Icon = "🍌",
            WhatToThrow = new[]
            {
                "Restes de cuisine (épluchures de plantain, manioc, macabo, fruits)",
                "Feuilles et déchets verts du jardin"
            },
            Rules = new[]
            {
                "Déchets biodégradables destinés au compostage ou aux circuits de valorisation agricole locaux.",
                "Évitez absolument d'y mélanger des sachets plastiques."
            }
        },
        new SortingCategory
        {
            Key = "glass", Title = "Verre & métaux", Icon = "🍾",
            WhatToThrow = new[]
            {
                "Bouteilles en verre et pots",
                "Canettes de boissons et boîtes de conserve métalliques"
            },
            Rules = new[]
            {
                "Retirez les bouchons et couvercles.",
                "Veillez à ne pas jeter de verre brisé en vrac pour protéger les agents de collecte."
            }
        },
        new SortingCategory
        {
            Key = "plastic", Title = "Plastiques & Emballages", Icon = "♻️",
            WhatToThrow = new[]
            {
                "Bouteilles d'eau minérale et jus",
                "Sachets plastiques propres et emballages souples"
            },
            Rules = new[]
            {
                "Videz et rincez légèrement les bouteilles.",
                "Écrasez les bouteilles pour économiser de l'espace dans les bacs de collecte de Douala.",
                "Bonus : ça rapporte des Green Points !"
            }
        },
    };

    public IReadOnlyList<SortingCategory> GetAll() => _items;
    public SortingCategory? GetByKey(string key) => _items.FirstOrDefault(c => c.Key == key);
}
