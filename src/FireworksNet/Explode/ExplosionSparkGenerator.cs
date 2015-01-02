﻿using System;
using System.Collections.Generic;
using FireworksNet.Model;
using FireworksNet.Randomization;

namespace FireworksNet.Explode
{
	/// <summary>
	/// Conventional Explosion spark generator, as described in 2010 paper
	/// </summary>
	public class ExplosionSparkGenerator : SparkGenerator
	{
		private readonly IEnumerable<Dimension> dimensions;
		private readonly IRandom randomizer;

        public override FireworkType GeneratedSparkType { get { return FireworkType.ExplosionSpark; } }

		public ExplosionSparkGenerator(IEnumerable<Dimension> dimensions, IRandom randomizer)
		{
			if (dimensions == null)
			{
				throw new ArgumentNullException("dimensions");
			}

			if (randomizer == null)
			{
                throw new ArgumentNullException("randomizer");
			}

			this.dimensions = dimensions;
			this.randomizer = randomizer;
		}

		public override Firework CreateSpark(Explosion explosion)
		{
			Firework spark = new Firework(GeneratedSparkType, explosion.StepNumber, explosion.ParentFirework.Coordinates);

			double offsetDisplacement = explosion.Amplitude * randomizer.GetNext(-1.0, 1.0);
			foreach (Dimension dimension in dimensions)
			{
				if ((int)Math.Round(randomizer.GetNext(0.0, 1.0), MidpointRounding.AwayFromZero) == 1) // Coin flip
				{
					spark.Coordinates[dimension] += offsetDisplacement;
					if (!dimension.IsValueInBounds(spark.Coordinates[dimension]))
					{
						spark.Coordinates[dimension] = dimension.VariationRange.Minimum + Math.Abs(spark.Coordinates[dimension]) % dimension.VariationRange.Length;
					}
				}
			}

			return spark;
		}
	}
}