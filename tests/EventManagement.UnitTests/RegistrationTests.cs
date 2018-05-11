﻿using Xunit;

using losol.EventManagement.Domain;
using System;
using System.Collections.Generic;
using static losol.EventManagement.Domain.Order;
using System.Linq;
using static losol.EventManagement.Domain.Registration;

namespace losol.EventManagement.UnitTests
{
	public class RegistrationTests
	{
		public class CreateOrder_Should
		{
			[Fact]
			public void Succeed_WhenProvidedWithOnlyProducts()
			{
				// Arrange
				// Registration for an event#1 with Product#1
				var registration = new Registration
				{
					EventInfoId = 1,
					EventInfo = new EventInfo
					{
						EventInfoId = 1,
						Products = new List<Product>
						{
							new Product { ProductId = 1, EventInfoId = 1 }
						}
					}
				};
				// Products that belong to the event being registered for
				var dto = new List<OrderDTO>
				{
					new OrderDTO
					{
						Product = new Product { ProductId = 1, EventInfoId = 1, Price = 1000 }
					}
				};

				// Act
				registration.CreateOrder(dto);

				// Assert
				Assert.NotNull(registration.Orders);
				Assert.Equal(dto[0].Product.Price, registration.Orders[0].OrderLines[0].Price);
			}

			[Fact]
			public void Succeed_WhenProvidedWithProductsAndVariants()
			{
				// Arrange
				// Registration for an event#1 with Product#1
				var registration = new Registration
				{
					EventInfoId = 1,
					EventInfo = new EventInfo
					{
						EventInfoId = 1,
						Products = new List<Product>
						{
							new Product
							{
								ProductId = 1,
								EventInfoId = 1,
								ProductVariants = new List<ProductVariant>
								{
									new ProductVariant
									{
										ProductVariantId = 1,
										ProductId = 1
									}
								}
							}
						}
					}
				};
				// Products that belong to the event being registered for
				var dto = new List<OrderDTO>
				{
					new OrderDTO
					{
						Product = new Product { ProductId = 1, EventInfoId = 1, Price = 1000 },
						Variant = new ProductVariant { ProductId = 1, ProductVariantId = 1, Price = 2000 }
					}
				};

				//
				// Act
				registration.CreateOrder(dto);

				//
				// Assert
				Assert.NotNull(registration.Orders);
				Assert.Equal(dto[0].Variant.Price, registration.Orders[0].OrderLines[0].Price);
			}

			[Fact]
			public void ThrowException_WhenProvidedWithNull()
			{
				// Arrange
				var registration = new Registration();

				// Act & Assert
				Assert.Throws<ArgumentNullException>(() => registration.CreateOrder(null));
			}

			[Fact]
			public void ThrowException_WhenProvidedWithInvalidProducts()
			{
				// Arrange
				// Registration for an event#1 with Product#1
				var registration = new Registration
				{
					EventInfoId = 1,
					EventInfo = new EventInfo
					{
						EventInfoId = 1,
						Products = new List<Product>
						{
							new Product { ProductId = 1, EventInfoId = 1 }
						}
					}
				};
				// Products that don't belong to the event being registered for
				var products = new List<Product>
				{
					new Product { ProductId = 2, EventInfoId = 2 }
				};
				var dto = new List<OrderDTO>
				{
					new OrderDTO
					{
						Product = new Product { ProductId = 2, EventInfoId = 2 }
					}
				};

				// Act & Assert
				Assert.Throws<ArgumentException>(() => registration.CreateOrder(dto));
			}

			[Fact]
			public void ThrowException_WhenProvidedWithInvalidProductVariants()
			{
				// Arrange
				// Registration for an event#1 with Product#1
				var registration = new Registration
				{
					EventInfoId = 1,
					EventInfo = new EventInfo
					{
						EventInfoId = 1,
						Products = new List<Product>
						{
							new Product
							{
								ProductId = 1,
								EventInfoId = 1,
								ProductVariants = new List<ProductVariant>
								{
									new ProductVariant
									{
										ProductId = 1,
										ProductVariantId = 1
									}
								}
							}
						}
					}
				};
				// Products that don't belong to the event being registered for
				var dto = new List<OrderDTO>
				{
					new OrderDTO
					{
						Product = new Product { ProductId = 2, EventInfoId = 1 },
						Variant = new ProductVariant { ProductId = 1, ProductVariantId = 2 }
					}
				};

				// Act & Assert
				Assert.Throws<ArgumentException>(() => registration.CreateOrder(dto));
			}
		}

		public class Verify_Should
		{
			[Fact]
			public void SucceedWhenNotVerified()
			{
				Registration registration = new Registration();
				var expected = true;

				registration.Verify();
				var actual = registration.Verified;

				Assert.Equal(expected, actual);
			}

			[Fact]
			public void SucceedWhenAlreadyVerified()
			{
				Registration registration = new Registration { Verified = true, Status = RegistrationStatus.Verified };


				registration.Verify();


				Assert.True(registration.Verified);
				Assert.Equal(RegistrationStatus.Verified, registration.Status);
			}
		}

		public class RegisterAttendance_Should
		{
			[Fact]
			public void SucceedWhenNotNotAttended()
			{
				Registration registration = new Registration();
				var expected = RegistrationStatus.Attended;

				registration.MarkAsAttended();
				var actual = registration.Status;

				Assert.Equal(expected, actual);
			}

			[Fact]
			public void SucceedWhenAlreadyAttended()
			{
				Registration registration = new Registration { Status = RegistrationStatus.Attended };
				var expected = RegistrationStatus.Attended;

				registration.MarkAsAttended();
				var actual = registration.Status;

				Assert.Equal(expected, actual);
			}
		}


		public class CreateOrUpdateOrder_Should
		{

			[Fact]
			public void CreateNewOrderIfExistingOrdersAreInvoiced()
			{
				// Arrange
				var registration = new Registration
				{
					Orders = new List<Order> {
						new Order {
							OrderLines = new List<OrderLine> {
								new OrderLine { ProductId = 1 }
							}
						}
					}
				};
				foreach(var o in registration.Orders)
				{
					o.MarkAsVerified();
					o.MarkAsInvoiced();
				}
				var dto = new List<OrderDTO>
				{
					new OrderDTO
					{
						Product = new Product { ProductId = 2 }
					}
				};

				// Act
				registration.CreateOrUpdateOrder(dto);

				// Assert
				Assert.Equal(2, registration.Orders.Count);
			}

			[Fact]
			public void UpdateOrderIfExistingOrdersAreNotInvoiced()
			{
				// Arrange
				var registration = new Registration
				{
					Orders = new List<Order> {
						new Order {
							OrderLines = new List<OrderLine> {
								new OrderLine { ProductId = 1 }
							}
						}
					}
				};
				var dto = new List<OrderDTO>
				{
					new OrderDTO
					{
						Product = new Product { ProductId = 2 }
					}
				};

				// Act
				registration.CreateOrUpdateOrder(dto);

				// Assert
				Assert.Single(registration.Orders);
				Assert.Equal(2, registration.Orders.First().OrderLines.Count);
			}

			[Fact]
			public void SucceedIfProductExistsInCancelledOrder()
			{
				// Arrange
				var registration = new Registration
				{
					Orders = new List<Order> {
						new Order {
							OrderLines = new List<OrderLine> {
								new OrderLine { ProductId = 1 }
							}
						}
					}
				};
				registration.Orders.First().MarkAsCancelled();
				var dto = new List<OrderDTO>
				{
					new OrderDTO
					{
						Product = new Product { ProductId = 1 }
					}
				};

				// Act
				registration.CreateOrUpdateOrder(dto);

				// Assert
				Assert.Equal(2, registration.Orders.Count);
			}

			[Fact]
			public void CreditOrderLineIfProductExistsInInvoicedOrder()
			{
				// Arrange
				var registration = new Registration
				{
					Orders = new List<Order> {
						new Order {
							OrderLines = new List<OrderLine> {
								new OrderLine
								{
									ProductId = 1,
									ProductVariantId = 1,
									Price = 100,
									Product = new Product
									{
										ProductId = 1
									},
									ProductVariant = new ProductVariant
									{
										ProductVariantId = 1,
										ProductId = 1
									}
								},
								new OrderLine
								{
									ProductId = 2,
									Price = 100,
									Product = new Product
									{
										ProductId = 2
									}
								}
							}
						}
					}
				};
				registration.Orders.First().MarkAsVerified();
				registration.Orders.First().MarkAsInvoiced();

				var dto = new List<OrderDTO>
				{
					new OrderDTO
					{
						Product = new Product { ProductId = 1, Price = 100 },
						Variant = new ProductVariant { ProductVariantId = 2, ProductId = 1, Price = 100 }
					}
				};

				// Act
				registration.CreateOrUpdateOrder(dto);

				// Assert
				var last = registration.Orders.Last();
				Assert.Equal(2, registration.Orders.Count);
                Assert.Equal(2, last.OrderLines.Count);
				Assert.Equal(0m, last.TotalAmount);
			}

			[Fact]
			public void CreditAndRemoveProductsFromInvoicedOrder()
			{
				// Arrange
				var registration = new Registration
				{
					Orders = new List<Order> {
						new Order {
							OrderLines = new List<OrderLine> {
								new OrderLine
								{
									ProductId = 1,
									ProductVariantId = 1,
									Price = 100,
									Quantity = 5,
									Product = new Product
									{
										ProductId = 1
									},
									ProductVariant = new ProductVariant
									{
										ProductVariantId = 1,
										ProductId = 1
									}
								}
							}
						}
					}
				};
				registration.Orders.First().MarkAsVerified();
				registration.Orders.First().MarkAsInvoiced();

				var dto = new List<OrderDTO>
				{
					new OrderDTO
					{
						Product = new Product { ProductId = 1, Price = 100 },
						Quantity = 0
					}
				};

				// Act
				registration.CreateOrUpdateOrder(dto);

				// Assert
				var last = registration.Orders.Last();
				Assert.Equal(-500m, last.TotalAmount);
				Assert.Single(last.OrderLines); // only the refund orderline should exist
			}

		}

		public class HasOrder_Should
		{
			[Fact]
			public void ReturnFalseIfNull()
			{
				var registration = new Registration { };
				Assert.False(registration.HasOrder);
			}

			[Fact]
			public void ReturnFalseIfZero()
			{
				var registration = new Registration { Orders = new List<Order>() };
				Assert.False(registration.HasOrder);
			}

			[Fact]
			public void ReturnTrueIfHasOrder()
			{
				var registration = new Registration {
					Orders = new List<Order>() {
						new Order()
					}
				};
				Assert.True(registration.HasOrder);
			}
		}
	}
}
